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
        public double DisbalanceOriginal;
        public double Disbalance;
        public double[] reconciledValuesArray;
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
            Balance();
        }
        private void Balance() 
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
                
            BalanceOutput balanceOutput = new BalanceOutput();
            List<OutputVariables> balanceOutputVariables = new List<OutputVariables>();
            for (int i = 0; i < solution.Length; i++)
            {
                InputVariables outputVariable = inputData.BalanceInputVariables[i];
                balanceOutputVariables.Add(new OutputVariables() { id = outputVariable.id, name = outputVariable.name, value = solution[i],source = outputVariable.sourceId, target = outputVariable.destinationId });
            }
            balanceOutput.BalanceOutputVariables = balanceOutputVariables;
            balanceOutput.DisbalanceOriginal = disbalanceOriginal;
            balanceOutput.Disbalance = disbalance;
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
        public double StartGlobalTest(double[] x0, double[,] a, double[] measurability, double[] tolerance)
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
    }
}
