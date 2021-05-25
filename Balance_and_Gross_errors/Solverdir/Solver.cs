using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Balance_and_Gross_errors.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Balance_and_Gross_errors.Graphdir;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;
using MathNet.Numerics.Distributions;
using System.IO;
using Accord.Math.Optimization;
using TreeCollections;
using Gurobi;

namespace Balance_and_Gross_errors.Solverdir
{
    public class Solver
    {
        private int countOfThreads; // Количество потоков

        private SparseVector measuredValues;              // Вектор измеренных значений (x0)
        private SparseMatrix measureIndicator;            // Матрица измеряемости (I)
        private SparseMatrix standardDeviation;           // Матрица метрологической погрешности (W)
        private SparseMatrix incidenceMatrix;             // Матрица инцидентности / связей
        private SparseVector reconciledValues;            // Вектор b
        private DenseVector metrologicRangeUpperBound;   // Вектор верхних ограничений вектора x
        private DenseVector metrologicRangeLowerBound;   // Вектор нижних ограничений вектора x
        private DenseVector technologicRangeUpperBound;  // Вектор верхних ограничений вектора x
        private DenseVector technologicRangeLowerBound;  // Вектор нижних ограничений вектора x
        private SparseMatrix H;                           // H = I * W
        private SparseVector dVector;                     // d = H * x0
        private BalanceInput inputData;
        public double GTR;
        public double GLR;
        public double[] reconciledValuesArray;
        public BalanceOutput balanceOutput;
        public List<OutputVariables> balanceOutputVariables;
        private DenseVector absTolerance;                //вектор абсолютной погрешности
        public double[] sol;
        public double[] corr;
        public double GlrTime;
        public Solver(BalanceInput balanceInput)
        {

            this.inputData = balanceInput;
            countOfThreads = balanceInput.BalanceInputVariables.Count();// Инициализация количества потоков
            Graph graph = new Graph(balanceInput);
            incidenceMatrix = SparseMatrix.OfArray(graph.getIncidenceMatrix(balanceInput));// Матрица инцидентности
            // Инициализация вектора измеренных значений ( x0 )
            measuredValues = new SparseVector(countOfThreads);
            double[] tol = new double[countOfThreads];
            // Инициализация вектора верхних ограничений вектора x
            metrologicRangeUpperBound = new DenseVector(countOfThreads);
            technologicRangeUpperBound = new DenseVector(countOfThreads);
            // Инициализация вектора нижних ограничений вектора x
            metrologicRangeLowerBound = new DenseVector(countOfThreads);
            technologicRangeLowerBound = new DenseVector(countOfThreads);
            // Инициализация вектора абсолютных погрешностей
            absTolerance = new DenseVector(countOfThreads);
            //double[] tol = new double[countOfThreads];
            double[] measIndicator = new double[countOfThreads];
            for (int i = 0; i < countOfThreads; i++)
            {
                InputVariables variables = balanceInput.BalanceInputVariables[i];
                measuredValues[i] = variables.measured;
                // Определение матрицы измеряемости
                if (variables.isMeasured)
                    measIndicator[i] = 1.0;
                else measIndicator[i] = 0.0;
                // Определение матрицы метрологической погрешности
                if (!variables.isMeasured) tol[i] = 1.0;
                else
                {
                    double tolerance = 1.0 / Math.Pow(variables.tolerance, 2);
                    if (Double.IsInfinity(tolerance))
                    {
                        tolerance = 1.0;
                    }
                    if (Double.IsNaN(tolerance))
                    {
                        throw new Exception("Exception: NaN Value");
                    }
                    tol[i] = tolerance;
                }

                // Определение вектора верхних ограничений вектора x
                metrologicRangeUpperBound[i] = variables.metrologicUpperBound;
                technologicRangeUpperBound[i] = variables.technologicUpperBound;
                // Определение вектора нижних ограничений вектора x
                metrologicRangeLowerBound[i] = variables.metrologicLowerBound;
                technologicRangeLowerBound[i] = variables.technologicLowerBound;
                absTolerance[i] = variables.tolerance;
            }
            measureIndicator = SparseMatrix.OfDiagonalArray(countOfThreads, countOfThreads, measIndicator);
            standardDeviation = SparseMatrix.OfDiagonalArray(countOfThreads, countOfThreads, tol);

            H = new SparseMatrix(countOfThreads, countOfThreads);
            H = measureIndicator * standardDeviation;
            dVector = new SparseVector(countOfThreads);
            dVector = H * (-1) * measuredValues;
            // Инициализация вектора b
            reconciledValues = new SparseVector(incidenceMatrix.RowCount);
            GlobalTest();
        }
        public void BalanceAccord()
        {
            var func = new QuadraticObjectiveFunction(H.ToArray(), dVector.ToArray());
            var constraints = new List<LinearConstraint>();
            //добавление ограничений узлов
            for (var j = 0; j < measuredValues.ToArray().Length; j++)
            {
                if (inputData.balanceSettings.balanceSettingsConstraints == 0 || measureIndicator[j, j] == 0.0)
                {
                    constraints.Add(new LinearConstraint(1)
                    {
                        VariablesAtIndices = new[] { j },
                        ShouldBe = ConstraintType.GreaterThanOrEqualTo,
                        Value = inputData.BalanceInputVariables[j].technologicLowerBound
                    });

                    constraints.Add(new LinearConstraint(1)
                    {
                        VariablesAtIndices = new[] { j },
                        ShouldBe = ConstraintType.LesserThanOrEqualTo,
                        Value = inputData.BalanceInputVariables[j].technologicUpperBound
                    });
                }
                else
                {
                    constraints.Add(new LinearConstraint(1)
                    {
                        VariablesAtIndices = new[] { j },
                        ShouldBe = ConstraintType.GreaterThanOrEqualTo,
                        Value = inputData.BalanceInputVariables[j].metrologicLowerBound
                    });

                    constraints.Add(new LinearConstraint(1)
                    {
                        VariablesAtIndices = new[] { j },
                        ShouldBe = ConstraintType.LesserThanOrEqualTo,
                        Value = inputData.BalanceInputVariables[j].metrologicUpperBound
                    });
                }
            }
            //Ограничения для решения задачи баланса
            for (var j = 0; j < reconciledValues.ToArray().Length; j++)
            {
                var notNullElements = Array.FindAll(incidenceMatrix.ToArray().GetRow(j), x => Math.Abs(x) > 0.0000001);
                var notNullElementsIndexes = new List<int>();
                for (var k = 0; k < measuredValues.ToArray().Length; k++)
                {
                    if (Math.Abs(incidenceMatrix[j, k]) > 0.0000001)
                    {
                        notNullElementsIndexes.Add(k);
                    }
                }

                constraints.Add(new LinearConstraint(notNullElements.Length)
                {
                    VariablesAtIndices = notNullElementsIndexes.ToArray(),
                    CombinedAs = notNullElements,
                    ShouldBe = ConstraintType.EqualTo,
                    Value = reconciledValues[j]
                });
            }

            var solver = new GoldfarbIdnani(func, constraints);
            DateTime CalculationTimeStart = DateTime.Now;
            if (!solver.Minimize()) throw new ApplicationException("Failed to solve balance task.");
            DateTime CalculationTimeFinish = DateTime.Now;
            double disbalanceOriginal = incidenceMatrix.Multiply(measuredValues).Subtract(reconciledValues).ToArray().Euclidean();
            double disbalance = incidenceMatrix.Multiply(SparseVector.OfVector(new DenseVector(solver.Solution))).Subtract(reconciledValues).ToArray().Euclidean();
            double[] solution = new double[countOfThreads];
            sol = new double[countOfThreads];
            for (int i = 0; i < solution.Length; i++)
            {
                solution[i] = solver.Solution[i];
                sol[i] = solution[i];
            }

            balanceOutput = new BalanceOutput();
            balanceOutputVariables = new List<OutputVariables>();
            for (int i = 0; i < solution.Length; i++)
            {
                InputVariables outputVariable = inputData.BalanceInputVariables[i];
                balanceOutputVariables.Add(new OutputVariables()
                {
                    id = outputVariable.id,
                    name = outputVariable.name,
                    value = solution[i],
                    source = outputVariable.sourceId,
                    target = outputVariable.destinationId,
                    upperBound = (inputData.balanceSettings.balanceSettingsConstraints == 0 || measureIndicator[i, i] == 0.0) ? technologicRangeUpperBound[i] : metrologicRangeUpperBound[i],
                    lowerBound = (inputData.balanceSettings.balanceSettingsConstraints == 0 || measureIndicator[i, i] == 0.0) ? technologicRangeLowerBound[i] : metrologicRangeLowerBound[i]
                });
            }
            balanceOutput.CalculationTime = (CalculationTimeFinish - CalculationTimeStart).TotalSeconds;
            balanceOutput.balanceOutputVariables = balanceOutputVariables;
            balanceOutput.DisbalanceOriginal = disbalanceOriginal;
            balanceOutput.Disbalance = disbalance;
            balanceOutput.GlobaltestValue = GTR;
            balanceOutput.Status = "Success";
        }

        public void BalanceGurobi()
        {
            GRBEnv env = new GRBEnv();
            GRBModel model = new GRBModel(env);
            DateTime CalculationTimeStart;
            DateTime CalculationTimeFinish;
            double[] results = new double[measuredValues.ToArray().Length];
            if (inputData.balanceSettings.balanceSettingsConstraints == BalanceSettings.BalanceSettingsConstraints.TECHNOLOGIC)
            {
                //Create variables
                GRBVar[] varsTechnologic = new GRBVar[measuredValues.ToArray().Length];
                for (int i = 0; i < varsTechnologic.Length; i++)
                {
                    varsTechnologic[i] = model.AddVar(technologicRangeLowerBound[i], technologicRangeUpperBound[i], 0.0, GRB.CONTINUOUS, "x" + i);
                }
                //Set objective
                GRBQuadExpr objTechnologic = new GRBQuadExpr();
                for (int i = 0; i < varsTechnologic.Length; i++)
                {
                    objTechnologic.AddTerm(H[i, i] / 2.0, varsTechnologic[i], varsTechnologic[i]);
                }
                for (int i = 0; i < varsTechnologic.Length; i++)
                {
                    objTechnologic.AddTerm(dVector[i], varsTechnologic[i]);
                }
                model.SetObjective(objTechnologic);
                //Add constraints
                GRBLinExpr expr;
                for (int i = 0; i < incidenceMatrix.RowCount; i++)
                {
                    expr = new GRBLinExpr();
                    for (int j = 0; j < incidenceMatrix.ColumnCount; j++)
                    {
                        expr.AddTerm(incidenceMatrix[i, j], varsTechnologic[j]);
                    }
                    model.AddConstr(expr, GRB.EQUAL, 0.0, "c" + i);
                }
                // Optimize model
                CalculationTimeStart = DateTime.Now;
                model.Optimize();
                CalculationTimeFinish = DateTime.Now;
                results = new double[varsTechnologic.Length];
                for (int i = 0; i < results.Length; i++)
                {
                    results[i] = varsTechnologic[i].Get(GRB.DoubleAttr.X);
                }
            }
            else
            {
                //Create variables
                GRBVar[] varsMetrologic = new GRBVar[measuredValues.ToArray().Length];
                for (int i = 0; i < varsMetrologic.Length; i++)
                {
                    if (measureIndicator[i, i] == 0)
                    {
                        varsMetrologic[i] = model.AddVar(technologicRangeLowerBound[i], technologicRangeUpperBound[i], 0.0, GRB.CONTINUOUS, "x" + i);
                    }
                    else
                    {
                        varsMetrologic[i] = model.AddVar(metrologicRangeLowerBound[i], metrologicRangeUpperBound[i], 0.0, GRB.CONTINUOUS, "x" + i);
                    }

                }
                //Set objective
                GRBQuadExpr objMetroologic = new GRBQuadExpr();
                for (int i = 0; i < varsMetrologic.Length; i++)
                {
                    objMetroologic.AddTerm(H[i, i] / 2.0, varsMetrologic[i], varsMetrologic[i]);
                }
                for (int i = 0; i < varsMetrologic.Length; i++)
                {
                    objMetroologic.AddTerm(dVector[i], varsMetrologic[i]);
                }
                model.SetObjective(objMetroologic);
                //Add constraints
                GRBLinExpr expr;
                for (int i = 0; i < incidenceMatrix.RowCount; i++)
                {
                    expr = new GRBLinExpr();
                    for (int j = 0; j < incidenceMatrix.ColumnCount; j++)
                    {
                        expr.AddTerm(incidenceMatrix[i, j], varsMetrologic[j]);
                    }
                    model.AddConstr(expr, GRB.EQUAL, 0.0, "c" + i);
                }
                // Optimize model
                CalculationTimeStart = DateTime.Now;
                model.Optimize();
                CalculationTimeFinish = DateTime.Now;
                results = new double[varsMetrologic.Length];
                for (int i = 0; i < results.Length; i++)
                {
                    results[i] = varsMetrologic[i].Get(GRB.DoubleAttr.X);
                }

            }
            model.Dispose();
            env.Dispose();

            double disbalanceOriginal = incidenceMatrix.Multiply(measuredValues).Subtract(reconciledValues).ToArray().Euclidean();
            double disbalance = incidenceMatrix.Multiply(SparseVector.OfVector(new DenseVector(results))).Subtract(reconciledValues).ToArray().Euclidean();
            balanceOutput = new BalanceOutput();
            balanceOutputVariables = new List<OutputVariables>();
            for (int i = 0; i < results.Length; i++)
            {
                InputVariables outputVariable = inputData.BalanceInputVariables[i];
                balanceOutputVariables.Add(new OutputVariables()
                {
                    id = outputVariable.id,
                    name = outputVariable.name,
                    value = results[i],
                    source = outputVariable.sourceId,
                    target = outputVariable.destinationId,
                    upperBound = (inputData.balanceSettings.balanceSettingsConstraints == 0 || measureIndicator[i, i] == 0.0) ? technologicRangeUpperBound[i] : metrologicRangeUpperBound[i],
                    lowerBound = (inputData.balanceSettings.balanceSettingsConstraints == 0 || measureIndicator[i, i] == 0.0) ? technologicRangeLowerBound[i] : metrologicRangeLowerBound[i]
                });
            }
            balanceOutput.CalculationTime = (CalculationTimeFinish - CalculationTimeStart).TotalSeconds;
            balanceOutput.balanceOutputVariables = balanceOutputVariables;
            balanceOutput.DisbalanceOriginal = disbalanceOriginal;
            balanceOutput.Disbalance = disbalance;
            balanceOutput.GlobaltestValue = GTR;
            balanceOutput.Status = "Success";
        }

        public void GlobalTest()
        {
            var x0 = measuredValues.ToArray();
            var a = incidenceMatrix.ToArray();
            var temp = new SparseVector(countOfThreads);
            for (int i = 0; i < countOfThreads; i++)
            {
                temp[i] = measureIndicator[i, i];
            }
            var measurability = temp.ToArray();
            var tolerance = absTolerance.ToArray();
            GTR = StartGlobalTest(x0, a, measurability, tolerance);
        }
        public double StartGlobalTest(double[] x0, double[,] a, double[] measurability, double[] tolerance)
        {
            var aMatrix = SparseMatrix.OfArray(a);
            var aTransposedMatrix = SparseMatrix.OfMatrix(aMatrix.Transpose());
            var x0Vector = SparseVector.OfEnumerable(x0);

            // Введение погрешностей по неизмеряемым потокам
            var xStd = SparseVector.OfEnumerable(tolerance) / 1.96;

            for (var i = 0; i < xStd.Count; i++)
            {
                if /*(Math.Abs(measurability[i]) < 0.0000001)*/(measurability[i] == 0.0)
                {
                    xStd[i] = Math.Pow(10, 2) * x0Vector.Maximum();
                }
            }

            var sigma = SparseMatrix.OfDiagonalVector(xStd.PointwisePower(2));
            // Вычисление вектора дисбалансов
            var r = aMatrix * x0Vector;
            var v = aMatrix * sigma * aTransposedMatrix;
            var vv = v.ToArray();
            vv = vv.PseudoInverse();
            v = SparseMatrix.OfArray(vv);
            var result = r * v * r.ToColumnMatrix();
            var chi = ChiSquared.InvCDF(aMatrix.RowCount, 1 - 0.05);
            // нормирование
            return result[0] / chi;
        }
        public BalanceOutput BalanceGurobiForGLR(double[] x0, double[,] a, double[,] h, double[] d, double[] technologicLowerBound, double[] technologicUpperBound, double[] metrologicLowerBound, double[] metrologicUpperBound)
        {
            try
            {
                GRBEnv env = new GRBEnv();
                GRBModel model = new GRBModel(env);
                double[] results = new double[x0.Length];
                if (inputData.balanceSettings.balanceSettingsConstraints == BalanceSettings.BalanceSettingsConstraints.TECHNOLOGIC)
                {
                    //Create variables
                    GRBVar[] varsTechnologic = new GRBVar[a.Columns()];
                    for (int i = 0; i < varsTechnologic.Length; i++)
                    {
                        varsTechnologic[i] = model.AddVar(technologicLowerBound[i], technologicUpperBound[i], 0.0, GRB.CONTINUOUS, "x" + i);
                    }
                    //Set objective
                    GRBQuadExpr objTechnologic = new GRBQuadExpr();
                    for (int i = 0; i < varsTechnologic.Length; i++)
                    {
                        objTechnologic.AddTerm(h[i, i] / 2.0, varsTechnologic[i], varsTechnologic[i]);
                    }
                    for (int i = 0; i < varsTechnologic.Length; i++)
                    {
                        objTechnologic.AddTerm(d[i], varsTechnologic[i]);
                    }
                    model.SetObjective(objTechnologic);
                    //Add constraints
                    GRBLinExpr expr;
                    for (int i = 0; i < a.Rows(); i++)
                    {
                        expr = new GRBLinExpr();
                        for (int j = 0; j < a.Columns(); j++)
                        {
                            expr.AddTerm(a[i, j], varsTechnologic[j]);
                        }
                        model.AddConstr(expr, GRB.EQUAL, 0.0, "c" + i);
                    }
                    // Optimize model
                    model.Optimize();
                    //results = new double[varsTechnologic.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        results[i] = varsTechnologic[i].Get(GRB.DoubleAttr.X);
                    }
                }
                else
                {
                    //Create variables
                    GRBVar[] varsMetrologic = new GRBVar[a.Columns()];
                    for (int i = 0; i < varsMetrologic.Length; i++)
                    {
                        varsMetrologic[i] = model.AddVar(metrologicLowerBound[i], metrologicUpperBound[i], 0.0, GRB.CONTINUOUS, "x" + i);
                    }
                    //Set objective
                    GRBQuadExpr objMetrologic = new GRBQuadExpr();
                    for (int i = 0; i < varsMetrologic.Length; i++)
                    {
                        objMetrologic.AddTerm(h[i, i] / 2.0, varsMetrologic[i], varsMetrologic[i]);
                    }
                    for (int i = 0; i < varsMetrologic.Length; i++)
                    {
                        objMetrologic.AddTerm(d[i], varsMetrologic[i]);
                    }
                    model.SetObjective(objMetrologic);
                    //Add constraints
                    GRBLinExpr expr;
                    for (int i = 0; i < a.Rows(); i++)
                    {
                        expr = new GRBLinExpr();
                        for (int j = 0; j < a.Columns(); j++)
                        {
                            expr.AddTerm(a[i, j], varsMetrologic[j]);
                        }
                        model.AddConstr(expr, GRB.EQUAL, 0.0, "c" + i);
                    }
                    // Optimize model
                    model.Optimize();
                    //results = new double[varsMetrologic.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        results[i] = varsMetrologic[i].Get(GRB.DoubleAttr.X);
                    }
                }

                model.Dispose();
                env.Dispose();

                BalanceOutput outb = new BalanceOutput();
                var balanceOutputVars = new List<OutputVariables>();
                for (int i = 0; i < measuredValues.Count; i++)
                {
                    InputVariables outputVariable = inputData.BalanceInputVariables[i];
                    balanceOutputVars.Add(new OutputVariables()
                    {
                        id = outputVariable.id,
                        name = outputVariable.name,
                        value = results[i],
                        source = outputVariable.sourceId,
                        target = outputVariable.destinationId
                    });
                }
                outb.balanceOutputVariables = balanceOutputVars;
                outb.GlobaltestValue = GTR;
                outb.Status = "Success";
                return outb;
            }
            catch (Exception e)
            {
                return new BalanceOutput
                {
                    Status = e.Message,
                };
            }
        }
        public /*static*/ ICollection<(int Input, int Output, int FlowNum, string FlowName)> GetExistingFlows(double[,] a)
        {
            var flows = new List<(int, int, int, string)>();
            for (var k = 0; k < a.Columns(); k++)
            {
                var column = a.GetColumn(k);

                var i = column.IndexOf(-1);
                var j = column.IndexOf(1);

                if (i == -1 || j == -1)
                {
                    continue;
                }
                var fname = inputData.BalanceInputVariables[k].name;
                flows.Add((i, j, k, fname));
            }

            return flows;
        }
        public (double[,], List<(int Input, int Output, int FlowNum, string FlowName)>) GlrTest(double[] x0, double[,] a, double[] measurability, double[] tolerance,
            List<(int, int, int, string)> flows, double globalTest)
        {
            var nodesCount = a.GetLength(0);
            corr = new double[countOfThreads];
            var glrTable = new double[nodesCount, nodesCount];

            foreach (var flow in flows)
            {
                var sum = 0.0;
                var correction = 0.0;
                var (i, j, l, _) = flow;

                // Добавляем новый поток в схеме
                var aColumn = new double[nodesCount];
                aColumn[i] = -1;
                aColumn[j] = 1;

                var aNew = a.InsertColumn(aColumn);

                var aRow = new double[x0.Length];
                for (int k = 0; k < x0.Length; k++)
                    aRow[k] = a[i, k];
                for (int k = 0; k < x0.Length; k++)
                {
                    if (k == l)
                        continue;
                    else
                    {
                        if (aRow[k] == 1)
                            sum += x0[k];
                        else if (aRow[k] == -1)
                            sum -= x0[k];
                    }
                }
                if (sum > 0.0) correction -= sum;
                else correction += sum;
                corr[l] = correction;
                if (inputData.balanceSettings.balanceSettingsConstraints == BalanceSettings.BalanceSettingsConstraints.METROLOGIC && ((measuredValues[l] + corr[l]) < metrologicRangeLowerBound[l] || (measuredValues[l] + corr[l]) > metrologicRangeLowerBound[l]))
                    continue;
                if (inputData.balanceSettings.balanceSettingsConstraints == BalanceSettings.BalanceSettingsConstraints.TECHNOLOGIC && ((measuredValues[l] + corr[l]) < technologicRangeLowerBound[l] || (measuredValues[l] + corr[l]) > technologicRangeUpperBound[l]))
                    continue;
                var x0New = x0.Append(0).ToArray();

                var measurabilityNew = measurability.Append(0).ToArray();
                var toleranceNew = tolerance.Append(0).ToArray();

                // Считаем тест и находим разницу
                glrTable[i, j] = globalTest - StartGlobalTest(x0New, aNew, measurabilityNew, toleranceNew);
            }


            return (glrTable, flows);
        }

        public (double[,], List<(int Input, int Output, int FlowNum, string FlowName)>) ParallelGlrTest(double[] x0, double[,] a, double[] measurability, double[] tolerance,
            List<(int, int, int, string)> flows, double globalTest)
        {
            var nodesCount = a.GetLength(0);
            corr = new double[countOfThreads];
            var glrTable = new double[nodesCount, nodesCount];

            //foreach (var flow in flows)
            Parallel.ForEach(flows, op => b(op));
            void b((int,int,int,string)ff)
            {
                var sum = 0.0;
                var correction = 0.0;
                var (i, j, l, _) =ff;

                // Добавляем новый поток в схеме
                var aColumn = new double[nodesCount];
                aColumn[i] = -1;
                aColumn[j] = 1;

                var aNew = a.InsertColumn(aColumn);

                var aRow = new double[x0.Length];
                for (int k = 0; k < x0.Length; k++)
                    aRow[k] = a[i, k];
                for (int k = 0; k < x0.Length; k++)
                {
                    if (k == l)
                        continue;
                    else
                    {
                        if (aRow[k] == 1)
                            sum += x0[k];
                        else if (aRow[k] == -1)
                            sum -= x0[k];
                    }
                }
                if (sum > 0.0) correction -= sum;
                else correction += sum;
                corr[l] = correction;
                if (inputData.balanceSettings.balanceSettingsConstraints == BalanceSettings.BalanceSettingsConstraints.METROLOGIC && ((measuredValues[l] + corr[l]) < metrologicRangeLowerBound[l] || (measuredValues[l] + corr[l]) > metrologicRangeLowerBound[l]))
                {
                    glrTable[i, j] = 0.0;
                    return;
                }
                //continue;
                if (inputData.balanceSettings.balanceSettingsConstraints == BalanceSettings.BalanceSettingsConstraints.TECHNOLOGIC && ((measuredValues[l] + corr[l]) < technologicRangeLowerBound[l] || (measuredValues[l] + corr[l]) > technologicRangeUpperBound[l]))
                {
                    glrTable[i, j] = 0.0;
                    return;
                }
                //continue;
                var x0New = x0.Append(0).ToArray();

                var measurabilityNew = measurability.Append(0).ToArray();
                var toleranceNew = tolerance.Append(0).ToArray();

                // Считаем тест и находим разницу
                glrTable[i, j] = globalTest - StartGlobalTest(x0New, aNew, measurabilityNew, toleranceNew);
            }


            return (glrTable, flows);
        }

        public (MutableEntityTreeNode<Guid, TreeElement>, List<(int Input, int Output, int FlowNum, string FlowName)>) StartGlr()
        {
            DateTime CalculationTimeStart;
            DateTime CalculationTimeFinish;
            var x0 = measuredValues.ToArray();
            var a = incidenceMatrix.ToArray();
            var metrU = metrologicRangeUpperBound.ToArray();
            var metrL = metrologicRangeLowerBound.ToArray();
            var techU = technologicRangeUpperBound.ToArray();
            var techL = technologicRangeLowerBound.ToArray();
            var temp = new SparseVector(countOfThreads);
            for (int i = 0; i < countOfThreads; i++)
            {
                temp[i] = measureIndicator[i, i];
            }
            var measurability = temp.ToArray();
            var tolerance = absTolerance.ToArray();

            var flows = GetExistingFlows(a).ToList();
            var nodesCount = a.Rows();
            var rootNode = new MutableEntityTreeNode<Guid, TreeElement>(x => x.Id, new TreeElement());
            var analyzingNode = rootNode;
            while (analyzingNode != null)
            {
                var newMeasurability = measurability;
                var newTolerance = tolerance;
                var newA = a;
                var newX0 = x0;
                var newmetrU = metrU;
                var newmetrL = metrL;
                var newtechU = techU;
                var newtechL = techL;
                var newh = H.ToArray();
                var newD = dVector.ToArray();
                //Добавляем уже сущ. потоки от родителя
                foreach (var (newI, newJ, newNum, newName) in analyzingNode.Item.Flows)
                {
                    var aColumn = new double[nodesCount];
                    aColumn[newI] = 1;
                    aColumn[newJ] = -1;

                    newMeasurability = newMeasurability.Append(0).ToArray();
                    newTolerance = newTolerance.Append(0).ToArray();

                    newX0 = newX0.Append(0).ToArray();
                    newmetrU = newmetrU.Append(metrologicRangeUpperBound[newNum]).ToArray();
                    newmetrL = newmetrL.Append(metrologicRangeLowerBound[newNum]).ToArray();
                    newtechU = newtechU.Append(technologicRangeUpperBound[newNum]).ToArray();
                    newtechL = newmetrL.Append(technologicRangeLowerBound[newNum]).ToArray();
                    var hColumn = new double[nodesCount + 1];
                    var hRow = new double[newX0.Length];
                    foreach (int elem in hColumn)
                        hColumn[elem] = 0;
                    foreach (int elem in hRow)
                        hRow[elem] = 0;
                    newh = newh.InsertColumn(hColumn);
                    newh = newh.InsertRow(hRow);
                    newD = newD.Append(0).ToArray();
                    newA = newA.InsertColumn(aColumn);
                }
                CalculationTimeStart = DateTime.Now;
                //Значение глобального теста
                var gTest = StartGlobalTest(newX0, newA, newMeasurability, newTolerance);

                //GLR
                var (glr, fl) = GlrTest(newX0, newA, newMeasurability, newTolerance, flows, gTest);
                //var (glr, fl) = ParallelGlrTest(newX0, newA, newMeasurability, newTolerance, flows, gTest);
                var (i, j) = glr.ArgMax();
                var ijvalue = glr[i, j];
                var check = BalanceGurobiForGLR(newX0, newA, newh, newD, newtechL, newtechU, newmetrL, newmetrU);
                //if (gTest >= 0.05)
                if (gTest >= 1)
                {
                    var flowIndex = fl[fl.FindIndex(x => x.Input == i && x.Output == j)].FlowNum;
                    var flowName = fl[fl.FindIndex(x => x.Input == i && x.Output == j)].FlowName;
                    var fname = inputData.BalanceInputVariables[flowIndex].name;
                    if (check.Status == "Success" && inputData.balanceSettings.balanceSettingsConstraints == BalanceSettings.BalanceSettingsConstraints.TECHNOLOGIC)
                    {
                        var node = new TreeElement(new List<(int, int, int, string)>(analyzingNode.Item.Flows), gTest);
                        analyzingNode = analyzingNode.AddChild(node);
                        node.Flows.Add((i, j, flowIndex, flowName));
                        //node.metrologicLowerBound = metrologicRangeLowerBound[flowIndex];
                        //node.metrologicUpperBound = metrologicRangeUpperBound[flowIndex];
                        //node.technologicLowerBound = technologicRangeLowerBound[flowIndex];
                        //node.technologicUpperBound = technologicRangeUpperBound[flowIndex];
                        inputData.BalanceInputVariables[flowIndex].metrologicLowerBound = corr[flowIndex] - tolerance[flowIndex];
                        inputData.BalanceInputVariables[flowIndex].metrologicUpperBound = corr[flowIndex] + tolerance[flowIndex];
                    }
                    if (check.Status != "Success" && inputData.balanceSettings.balanceSettingsConstraints == BalanceSettings.BalanceSettingsConstraints.METROLOGIC)
                    {
                        var node = new TreeElement(new List<(int, int, int, string)>(analyzingNode.Item.Flows), gTest);
                        analyzingNode = analyzingNode.AddChild(node);
                        node.Flows.Add((i, j, flowIndex, flowName));
                        //node.metrologicLowerBound = metrologicRangeLowerBound[flowIndex];
                        //node.metrologicUpperBound = metrologicRangeUpperBound[flowIndex];
                        //node.technologicLowerBound = technologicRangeLowerBound[flowIndex];
                        //node.technologicUpperBound = technologicRangeUpperBound[flowIndex];
                    }
                }
                else
                {
                    CalculationTimeFinish = DateTime.Now;
                    GlrTime = (CalculationTimeFinish - CalculationTimeStart).TotalSeconds;
                    analyzingNode.Item.GlobalTestValue = gTest;
                    analyzingNode = null;
                }
            }
            return (rootNode, flows);
        }

    }
}
