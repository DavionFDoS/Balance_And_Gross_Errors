using Balance_and_Gross_errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Balance_and_Gross_errors.Graphdir
{
    public class Graph
    {
        // Список вершин
        private List<Vertex> vertexList;
        // Матрица инцидентности
        private double[,] matrix;
        public Graph(BalanceInput balanceInput)
        {
            //if (balanceInput == null) throw new NullPointerException("balanceInput can't be null");

            vertexList = new List<Vertex>();

            // Определение количества потоков
            int countOfThreads = balanceInput.BalanceInputVariables.Count();
            // Создание вершин графа
            for (int i = 0; i < countOfThreads; i++)
            {
                string sourceId = balanceInput.BalanceInputVariables[i].sourceId;
                string destinationId = balanceInput.BalanceInputVariables[i].destinationId;
                if (sourceId != null)
                {
                    bool isExisted = false;
                    foreach (Vertex vertex in vertexList)
                    {
                        if (sourceId.Equals(vertex.Id))
                        {
                            isExisted = true;
                            break;
                        }
                    }
                    if (!isExisted)
                    {
                        vertexList.Add(new Vertex(sourceId));
                    }
                }
                else if (destinationId != null)
                {
                    bool isExisted = false;
                    foreach (Vertex vertex in vertexList)
                    {
                        if (destinationId.Equals(vertex.Id))
                        {
                            isExisted = true;
                            break;
                        }
                    }
                    if (!isExisted)
                    {
                        vertexList.Add(new Vertex(destinationId));
                    }
                }
            }

            // Определение количества вершин
            int countOfVertexes = vertexList.Count();

            // Инициализация матрицы инцидентности
            matrix = new double[countOfVertexes, countOfThreads];
            for (int thread = 0; thread < countOfThreads; thread++)
            {
                string sourceId = balanceInput.BalanceInputVariables[thread].sourceId;
                string destinationId = balanceInput.BalanceInputVariables[thread].destinationId;
                for (int vertex = 0; vertex < vertexList.Count(); vertex++)
                {
                    string vertexId = vertexList[vertex].Id;
                    if (destinationId != null && destinationId.Equals(vertexId))
                    {
                        matrix[vertex, thread] = 1;
                    }
                    if (sourceId != null && sourceId.Equals(vertexId))
                    {
                        matrix[vertex, thread] = -1;
                    }
                }
            }
        }
        public double[,] getMatrix()
        {
            return matrix;
        }
        public double[,] getIncidenceMatrix(BalanceInput balanceInput)
        {
            Graph graph = new Graph(balanceInput);
            return graph.getMatrix();
        }
    }
}
