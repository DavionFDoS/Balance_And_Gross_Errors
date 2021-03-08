using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Balance_and_Gross_errors.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Balance_and_Gross_errors.Graphdir;

namespace Balance_and_Gross_errors.Solver
{
    public class Solver
    {
        private int countOfThreads; // Количество потоков

        private DenseVector measuredValues;              // Вектор измеренных значений (x0)
        private DenseMatrix measureIndicator;            // Матрица измеряемости (I)
        private DenseMatrix standardDeviation;           // Матрица метрологической погрешности (W)
        private DenseMatrix incidenceMatrix;             // Матрица инцидентности / связей
        private DenseVector reconciledValues;            // Вектор сбалансированных значений (x)
        private DenseVector metrologicRangeUpperBound;   // Вектор верхних ограничений вектора x
        private DenseVector metrologicRangeLowerBound;   // Вектор нижних ограничений вектора x
        private DenseVector technologicRangeUpperBound;  // Вектор верхних ограничений вектора x
        private DenseVector technologicRangeLowerBound;  // Вектор нижних ограничений вектора x
        private DenseMatrix H;                           // H = I * W
        private DenseVector dVector;                     // d = H * x0
        private BalanceInput inputData;


        private DenseVector disbalanceVector; //r
        private DenseMatrix transposedIncedence;//at
        private DenseMatrix transposedDisbalance;//rt
        public Solver(BalanceInput balanceInput)
        {
            this.inputData = balanceInput;
            countOfThreads = balanceInput.BalanceInputVariables.Count();// Инициализация количества потоков
            Graph graph = new Graph(balanceInput);
            incidenceMatrix = DenseMatrix.OfArray(graph.getIncidenceMatrix(balanceInput));// Матрица инцидентности
            // Инициализация вектора измеренных значений ( x0 )
            measuredValues = new DenseVector(countOfThreads);
            // Инициализация матрицы измеряемости(I )
            measureIndicator = new DenseMatrix(countOfThreads);//квадратная
            // Инициализация матрицы метрологической погрешности ( 1 / t * t )
            standardDeviation = new DenseMatrix(countOfThreads);
            // Инициализация вектора верхних ограничений вектора x
            metrologicRangeUpperBound = new DenseVector(countOfThreads);
            technologicRangeUpperBound = new DenseVector(countOfThreads);
            // Инициализация вектора нижних ограничений вектора x
            metrologicRangeLowerBound = new DenseVector(countOfThreads);
            technologicRangeLowerBound = new DenseVector(countOfThreads);

            for (int i = 0; i < countOfThreads; i++)
            {
                InputVariables variables = balanceInput.BalanceInputVariables[i];

                // Определение вектора измеренных значений
                measuredValues[i] = variables.measured;
                // Определение матрицы измеряемости
                if (variables.isMeasured)
                    measureIndicator[i, i] = 1.0;
                else measureIndicator[i, i] = 0.0;
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

            }
            H = new DenseMatrix(countOfThreads);
            H = measureIndicator * standardDeviation;
            dVector = new DenseVector(countOfThreads);
            dVector = -H * measuredValues;
            // Инициализация вектора сбалансированных значений
            reconciledValues = new DenseVector(incidenceMatrix.RowCount);


        }
        public double GlobalTest()
        {// Инициализация вектора дисбалансов
            disbalanceVector = new DenseVector(incidenceMatrix.ColumnCount);//r
            transposedIncedence = new DenseMatrix(incidenceMatrix.RowCount);//at
            DenseMatrix V = new DenseMatrix(countOfThreads);
            double coef_delta = 1.96;
            double[] isMeas = new double[countOfThreads];//E
            double maxx0=-1000;
            for (int i = 0; i < countOfThreads; i++)
                maxx0 = Math.Max(maxx0, measuredValues[i]);
            double xStd = AbsTols / coef_delta;
            for (int i = 0; i < countOfThreads; i++)
            {
                isMeas[i] = measureIndicator[i, i];

                if (isMeas[i] == 0)
                    xStd = 10 * 10 * maxx0;
            }
            DiagonalMatrix xSigma = new DiagonalMatrix(countOfThreads, countOfThreads, xStd * xStd);//E

            //for (int i = 0; i < incidenceMatrix.RowCount; i++)
            //{
            //    for (int j = 0; j < incidenceMatrix.ColumnCount; j++)
            //    {
            //        if (incidenceMatrix[i, j] == 1)
            //            disbalanceVector[0, i] += measuredValues[i];
            //        else if (incidenceMatrix[i, j] == -1)
            //            disbalanceVector[0, i] -= measuredValues[i];
            //    }

            //}//r
            //for (int i = 0; i < incidenceMatrix.RowCount; i++)
            //    for (int j = 0; j < 1; j++)
            //        transposedDisbalanceVector[i, j] = disbalanceVector[j, i];//rt

            disbalanceVector = incidenceMatrix * measuredValues;//r
            for (int i = 0; i < incidenceMatrix.RowCount; i++)
                for (int j = 0; j < 1; j++)
                    transposedDisbalance[i, j] = disbalanceVector[j];//rt
            transposedIncedence = (DenseMatrix)incidenceMatrix.Transpose();//at

            V = (DenseMatrix)(incidenceMatrix * xSigma*transposedIncedence);//V
            double gt = disbalanceVector * transposedDisbalance * V;
            return 0;
        }
    }
}