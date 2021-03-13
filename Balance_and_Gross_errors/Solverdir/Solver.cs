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

namespace Balance_and_Gross_errors.Solverdir
{
    public class Solver
    {
        private int countOfThreads; // Количество потоков

        private SparseVector measuredValues;              // Вектор измеренных значений (x0)
        private SparseMatrix measureIndicator;            // Матрица измеряемости (I)
        private SparseMatrix standardDeviation;           // Матрица метрологической погрешности (W)
        private SparseMatrix incidenceMatrix;             // Матрица инцидентности / связей
        private SparseVector reconciledValues;            // Вектор сбалансированных значений (x)
        private DenseVector metrologicRangeUpperBound;   // Вектор верхних ограничений вектора x
        private DenseVector metrologicRangeLowerBound;   // Вектор нижних ограничений вектора x
        private DenseVector technologicRangeUpperBound;  // Вектор верхних ограничений вектора x
        private DenseVector technologicRangeLowerBound;  // Вектор нижних ограничений вектора x
        private SparseMatrix H;                           // H = I * W
        private SparseVector dVector;                     // d = H * x0
        private BalanceInput inputData;
        public double GTR;

        public double[] reconciledValuesArray;
        private DenseVector absTolerance;                //вектор абсолютной погрешности
        private DenseVector disbalanceVector; //r
        private DenseMatrix transposedIncedence;//at
        private DenseVector transposedDisbalance;//rt

        public Solver(BalanceInput balanceInput)
        {
            this.inputData = balanceInput;
            countOfThreads = balanceInput.BalanceInputVariables.Count();// Инициализация количества потоков
            Graph graph = new Graph(balanceInput);
            incidenceMatrix = SparseMatrix.OfArray(graph.getIncidenceMatrix(balanceInput));// Матрица инцидентности
            // Инициализация вектора измеренных значений ( x0 )
            measuredValues = new SparseVector(countOfThreads);
            // Инициализация матрицы измеряемости(I )
            measureIndicator = new SparseMatrix(countOfThreads);//квадратная
            // Инициализация матрицы метрологической погрешности ( 1 / t * t )
            standardDeviation = new SparseMatrix(countOfThreads);
            // Инициализация вектора верхних ограничений вектора x
            metrologicRangeUpperBound = new DenseVector(countOfThreads);
            technologicRangeUpperBound = new DenseVector(countOfThreads);
            // Инициализация вектора нижних ограничений вектора x
            metrologicRangeLowerBound = new DenseVector(countOfThreads);
            technologicRangeLowerBound = new DenseVector(countOfThreads);
            // Инициализация вектора абсолютных погрешностей
            absTolerance = new DenseVector(countOfThreads);

            for (int i = 0; i < countOfThreads; i++)
            {
                InputVariables variables = balanceInput.BalanceInputVariables[i];
                measuredValues[i] = variables.measured;
                // Определение матрицы измеряемости
                if (variables.isMeasured)
                    measureIndicator[i, i] = 1.0;
                else measureIndicator[i, i] = 0.0;// переделать
                // Определение матрицы метрологической погрешности
                if (!variables.isMeasured) standardDeviation[i, i] = 1.0;
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
                    standardDeviation[i, i] = tolerance;
                }
                // Определение вектора верхних ограничений вектора x
                metrologicRangeUpperBound[i] = variables.metrologicUpperBound;
                technologicRangeUpperBound[i] = variables.technologicUpperBound;
                // Определение вектора нижних ограничений вектора x
                metrologicRangeLowerBound[i] = variables.metrologicLowerBound;
                technologicRangeLowerBound[i] = variables.technologicLowerBound;
                absTolerance[i] = variables.tolerance;
            }
            H = new SparseMatrix(countOfThreads, countOfThreads);
            H = measureIndicator * standardDeviation;
            dVector = new SparseVector(countOfThreads);
            dVector = H * measuredValues;
            // Инициализация вектора сбалансированных значений
            reconciledValues = new SparseVector(countOfThreads);
            GTR = GlobalTest();
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
            string writePath = @"F:\Balance2\Balance_And_Gross_Errors\file.txt";
            using (StreamWriter sw = new StreamWriter(writePath, false, System.Text.Encoding.Default))
            {
                //sw.WriteLine(this.countOfThreads);

                for (int i = 0; i < countOfThreads; i++)
                {
                    //sw.WriteLine(dVector[i]);
                    //sw.WriteLine(H[i, i]);
                    // sw.WriteLine(this.measuredValues[i]);
                    //sw.WriteLine(this.measureIndicator[i,i]);
                    //sw.WriteLine(this.metrologicRangeUpperBound[i]);
                    //sw.WriteLine(this.metrologicRangeLowerBound[i]);
                    sw.WriteLine(absTolerance[i]);
                    //for (int j = 0; j < incidenceMatrix.ColumnCount; j++)
                    //    sw.Write(this.incidenceMatrix[i, j]+"\t");
                    //sw.Write("\n");
                }

            }
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


        //public double GlobalTest(BalanceInput balanceInput)
        //{
        //    double coef_delta = 1.96;
        //    double maxx0 = -1000;
        //    InputVariables variables;
        //    DiagonalMatrix xSigma;
        //    DenseMatrix V;
        //    DenseMatrix A;
        //    double xStd = 1.0;
        //    double[] isMeas = new double[countOfThreads];//E    
        //    for (int i = 0; i < countOfThreads; i++)
        //    {
        //        variables = balanceInput.BalanceInputVariables[i];
        //        // Определение вектора погрешностей
        //        absTolerance[i] = variables.tolerance;
        //        maxx0 = Math.Max(maxx0, measuredValues[i]);
        //        isMeas[i] = measureIndicator[i, i];
        //        if (isMeas[i] == 0)
        //            xStd = 10 * 10 * maxx0;
        //        xStd = absTolerance[i] / coef_delta;
        //    }
        //    xSigma = new DiagonalMatrix(countOfThreads, countOfThreads, xStd * xStd);//E
        //    //for (int i = 0; i < incidenceMatrix.RowCount; i++)
        //    //    for (int j = 0; j < 1; j++)
        //    //        transposedDisbalanceVector[i, j] = disbalanceVector[j, i];//rt

        //    // Инициализация вектора дисбалансов
        //    disbalanceVector = new DenseVector(incidenceMatrix.ColumnCount);//r
        //    transposedIncedence = new DenseMatrix(incidenceMatrix.RowCount);//at
        //    transposedDisbalance = new DenseVector(incidenceMatrix.RowCount);
        //    V = new DenseMatrix(countOfThreads);//V
        //    disbalanceVector = incidenceMatrix * measuredValues;//r
        //    double[] darray = new double[countOfThreads];
        //    darray = disbalanceVector.ToArray();
        //    double[,] rarray = new double[countOfThreads, countOfThreads];
        //    for (int i = 0; i < incidenceMatrix.RowCount; i++)
        //        for (int j = 0; j < incidenceMatrix.ColumnCount; j++)
        //            rarray[i, j] = 0.0;
        //    for (int i = 0; i < incidenceMatrix.RowCount; i++)
        //        for (int j = 0; j < 1; j++)
        //            rarray[i, j] = darray[i];

        //    transposedDisbalance = DenseVector.OfArray()
        //    //    for (int i = 0; i < incidenceMatrix.RowCount; i++)
        //    //for (int j = 0; j < 1; j++)
        //    //    transposedDisbalance[i,j] = darray[i];// disbalanceVector[i];

        //    transposedIncedence = (DenseMatrix)incidenceMatrix.Transpose();//at

        //    V = (DenseMatrix)(incidenceMatrix * xSigma*transposedIncedence);//V
        //    double[,] varray = new double[countOfThreads, countOfThreads];
        //    varray = V.ToArray();
        //    double[,] pinv = varray.PseudoInverse();
        //    V = DenseMatrix.OfArray(pinv);
        //    double gt = disbalanceVector * transposedDisbalance * V;
        //    double alpha = 0.05;
        //    int DegreeOfFreedom = incidenceMatrix.RowCount;
        //    var invchisq = new InverseChiSquareDistribution(DegreeOfFreedom);
        //    double GT_limit = invchisq.InverseDistributionFunction(1 - alpha);
        //    return gt;
        //}
    }
}
