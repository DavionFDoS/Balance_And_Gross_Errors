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
        public double DisbalanceOriginal;
        public double Disbalance;
        public double[] reconciledValuesArray;
        public BalanceOutput balanceOutput;
        public List<OutputVariables> balanceOutputVariables;
        private DenseVector absTolerance;                //вектор абсолютной погрешности
        public double[] sol;
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
            GTR = GlobalTest();
            //Balance();
        }
        public void Balance()
        {
            var func = new QuadraticObjectiveFunction(H.ToArray(), dVector.ToArray());
            var constraints = new List<LinearConstraint>();

            for (var j = 0; j < measuredValues.ToArray().Length; j++)
            {
                bool flag = inputData.BalanceInputVariables[j].useTechnologic;
                if (flag == true)
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
            if (!solver.Minimize()) throw new ApplicationException("Failed to solve balance task.");
            double disbalanceOriginal = incidenceMatrix.Multiply(measuredValues).Subtract(reconciledValues).ToArray().Euclidean();
            double disbalance = incidenceMatrix.Multiply(SparseVector.OfVector(new DenseVector(solver.Solution))).Subtract(reconciledValues).ToArray().Euclidean();
            DisbalanceOriginal = disbalanceOriginal;
            Disbalance = disbalance;
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
                    target = outputVariable.destinationId
                });
            }
            balanceOutput.balanceOutputVariables = balanceOutputVariables;
            balanceOutput.DisbalanceOriginal = disbalanceOriginal;
            balanceOutput.Disbalance = disbalance;
            balanceOutput.GlobaltestValue = GTR;
            balanceOutput.Status = "Success";
        }

        public double GlobalTest()
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

            return GTR;

        }
        public static double StartGlobalTest(double[] x0, double[,] a, double[] measurability, double[] tolerance)
        {
            var aMatrix = SparseMatrix.OfArray(a);
            var aTransposedMatrix = SparseMatrix.OfMatrix(aMatrix.Transpose());
            var x0Vector = SparseVector.OfEnumerable(x0);

            // Введение погрешностей по неизмеряемым потокам
            var xStd = SparseVector.OfEnumerable(tolerance) / 1.96;

            for (var i = 0; i < xStd.Count; i++)
            {
                if (Math.Abs(measurability[i]) < 0.0000001)
                {
                    xStd[i] = Math.Pow(10, 2) * x0Vector.Maximum();
                }
            }

            var sigma = SparseMatrix.OfDiagonalVector(xStd.PointwisePower(2));

            var r = aMatrix * x0Vector;
            var v = aMatrix * sigma * aTransposedMatrix;

            var result = r * v.PseudoInverse() * r.ToColumnMatrix();
            var chi = ChiSquared.InvCDF(aMatrix.RowCount, 1 - 0.05);

            return result[0] / chi;
        }
        public static ICollection<(int Input, int Output, int FlowNum)> GetExistingFlows(double[,] a)
        {
            var flows = new List<(int, int, int)>();
            for (var k = 0; k < a.Columns(); k++)
            {
                var column = a.GetColumn(k);

                var i = column.IndexOf(-1);
                var j = column.IndexOf(1);

                if (i == -1 || j == -1)
                {
                    continue;
                }

                flows.Add((i, j, k));
            }

            return flows;
        }
        public static double[,] GlrTest(double[] x0, double[,] a, double[] measurability, double[] tolerance,
            ICollection<(int, int, int)> flows, double globalTest)
        {
            var nodesCount = a.GetLength(0);

            var glrTable = new double[nodesCount, nodesCount];

            if (flows != null)
            {
                foreach (var flow in flows)
                {
                    var (i, j, _) = flow;

                    // Добавляем новый поток
                    var aColumn = new double[nodesCount];
                    aColumn[i] = 1;
                    aColumn[j] = -1;

                    var aNew = a.InsertColumn(aColumn);
                    var x0New = x0.Append(0).ToArray();
                    var measurabilityNew = measurability.Append(0).ToArray();
                    var toleranceNew = tolerance.Append(0).ToArray();

                    // Считаем тест и находим разницу
                    glrTable[i, j] = globalTest - StartGlobalTest(x0New, aNew, measurabilityNew, toleranceNew);
                }
            }
            else
            {
                for (var i = 0; i < nodesCount; i++)
                {
                    for (var j = i + 1; j < nodesCount; j++)
                    {
                        // Добавляем новый поток
                        var aColumn = new double[nodesCount];
                        aColumn[i] = 1;
                        aColumn[j] = -1;

                        var aNew = a.InsertColumn(aColumn);
                        var x0New = x0.Append(0).ToArray();
                        var measurabilityNew = measurability.Append(0).ToArray();
                        var toleranceNew = tolerance.Append(0).ToArray();

                        // Считаем тест и находим разницу
                        glrTable[i, j] = globalTest - StartGlobalTest(x0New, aNew, measurabilityNew, toleranceNew);
                    }
                }
            }

            return glrTable;
        }
        public (MutableEntityTreeNode<Guid, TreeElement>, List<(int Input, int Output, int FlowNum)>) GlrPrep()
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
            var GLR = StartGlr(x0, a, measurability, tolerance);
            return GLR;
        }
        public (MutableEntityTreeNode<Guid, TreeElement>, List<(int Input, int Output, int FlowNum)>) StartGlr(double[] x0, double[,] a, double[] measurability, double[] tolerance)
        {

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
                //Добавляем новые потоки
                foreach (var (newI, newJ) in analyzingNode.Item.Flows)
                {
                    var aColumn = new double[nodesCount];
                    aColumn[newI] = 1;
                    aColumn[newJ] = -1;

                    newMeasurability = newMeasurability.Append(0).ToArray();
                    newTolerance = newTolerance.Append(0).ToArray();

                    newX0 = newX0.Append(0).ToArray();
                    newA = newA.InsertColumn(aColumn);
                }
                //Значение глобального теста
                var gTest = StartGlobalTest(newX0, newA, newMeasurability,
                    newTolerance);

                //GLR
                var glr = GlrTest(newX0, newA, newMeasurability,
                    newTolerance, flows, gTest);
                var (i, j) = glr.ArgMax();
                if (glr[i, j] > 0 && gTest >= 1)
                {
                    //Создаем узел
                    var node = new TreeElement(new List<(int, int)>(analyzingNode.Item.Flows), gTest - glr[i, j]);

                    //Добавляем дочерний элемент
                    analyzingNode = analyzingNode.AddChild(node);
                    node.Flows.Add((i, j));
                }
                else
                {
                    //Переназначаем узел
                    analyzingNode = null;
                }
            }
            return (rootNode, flows);
        }

    }
}
