﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary.Model;
using Algorithms;
using log4net;
using BAModel.Model.Realization.EigenValue;


namespace Model.BAModel.Realization
{
    public class BAAnalyzer : AbstarctGraphAnalyzer
    {
        // Организация работы с лог файлом.
        protected static readonly new ILog log = log4net.LogManager.GetLogger(typeof(BAAnalyzer));

        // Контейнер, в котором содержится граф конкретной модели (BA).
        private BAContainer container;

        // Конструктор, получающий контейнер графа.
        public BAAnalyzer(BAContainer c)
        {
            log.Info("Creating BAAnalizer object.");
            container = c;
            Initialization();
        }

        // Контейнер, в котором содержится сгенерированный граф (полученный от генератора).
        public override AbstractGraphContainer Container
        {
            get { return container; }
            set { container = (BAContainer)value; }
        }

        // Возвращается средняя длина пути в графе. Реализовано.
        public override double GetAveragePath()
        {
            log.Info("Getting average path length.");

            if (-1 == avgPath)
            {
                CountEssentialOptions();
            }

            return Math.Round(avgPath, 14);
        }

        // Возвращается диаметр графа. Реализовано.
        public override int GetDiameter()
        {
            log.Info("Getting diameter.");

            if (-1 == diameter)
            {
                CountEssentialOptions();
            }

            return diameter;
        }

        // Возвращается число циклов длиной 3 в графе. Реализовано.
        public override long GetCycles3()
        {
            log.Info("Getting count of cycles - order 3.");

            if (-1 == avgPath)
            {
                CountEssentialOptions();
            }

            double cycles3 = 0;
            for (int i = 0; i < container.Size; ++i)
            {
                if (edgesBetweenNeighbours[i] != -1)
                    cycles3 += (int)edgesBetweenNeighbours[i];
            }

            if (cycles3 > 0 && cycles3 < 3)
                cycles3 = 1;
            else
                cycles3 /= 3;

            return (long)cycles3;
        }

        // Возвращается число циклов длиной 4 в графе. Реализовано.
        public override long GetCycles4()
        {
            log.Info("Getting count of cycles - order 4.");

            long count = 0;
            for (int i = 0; i < container.Size; i++)
                count += Get4OrderCyclesOfNode(i);

            return count / 4;
        }

        // Возвращается массив собственных значений матрицы смежности. Реализовано.
        public override ArrayList GetEigenValues()
        {
            log.Info("Getting eigen values array.");
            bool[,] m = container.GetMatrix();

            EigenValueUtils eg = new EigenValueUtils();
            
            try
            {
                return eg.CalculateEigenValue(m);

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new ArrayList();
            }
        }

        // Возвращается распределение длин между собственными значениями. Реализовано
        public override SortedDictionary<double, int> GetDistEigenPath()
        {
            log.Info("Getting distances between eigen values.");

            bool[,] m = container.GetMatrix();

            EigenValueUtils eg = new EigenValueUtils();


            try
            {
                eg.CalculateEigenValue(m);
                return eg.CalcEigenValuesDist();

            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new SortedDictionary<double, int>();
            }
        }

       
        // Возвращается степенное распределение графа. Реализовано.
        public override SortedDictionary<int, int> GetDegreeDistribution()
        {
            log.Info("Getting degree distribution.");

            return DegreeDistribution();
        }

        // Возвращается распределение коэффициентов кластеризации графа. Реализовано.
        public override SortedDictionary<double, int> GetClusteringCoefficient()
        {
            log.Info("Getting clustering coefficients.");

            if (-1 == avgPath)
            {
                CountEssentialOptions();
            }

            double clusteringCoefficient = 0;
            int iEdgeCountForFullness = 0, iNeighbourCount = 0;
            double iclusteringCoefficient = 0;
            SortedDictionary<double, int> m_iclusteringCoefficient = new SortedDictionary<double, int>();

            SortedDictionary<int, double> iclusteringCoefficientList = new SortedDictionary<int, double>();
            for (int i = 0; i < container.Size; ++i)
            {
                iNeighbourCount = container.CountVertexDegree(i);
                if (iNeighbourCount != 0)
                {
                    iEdgeCountForFullness = (iNeighbourCount == 1) ? 1 : iNeighbourCount * (iNeighbourCount - 1) / 2;
                    iclusteringCoefficient = (edgesBetweenNeighbours[i]) / iEdgeCountForFullness;
                    iclusteringCoefficientList[i] = Math.Round(iclusteringCoefficient, 4);
                    clusteringCoefficient += iclusteringCoefficient;
                }
                else
                    iclusteringCoefficientList[i] = 0;
            }

            clusteringCoefficient /= container.Size;

            for (int i = 0; i < container.Size; ++i)
            {
                double result = iclusteringCoefficientList[i];
                if (m_iclusteringCoefficient.Keys.Contains(result))
                    m_iclusteringCoefficient[iclusteringCoefficientList[i]]++;
                else
                    m_iclusteringCoefficient[iclusteringCoefficientList[i]] = 1;

                
            }
            return m_iclusteringCoefficient;
        }

        // Возвращается распределение длин минимальных путей в графе. Реализовано.
        public override SortedDictionary<int, int> GetMinPathDist()
        {
            log.Info("Getting minimal distances between vertices.");

            if (-1 == avgPath)
            {
                CountEssentialOptions();
            }

            return pathDistribution;
        }

        // Возвращается распределение чисел циклов. Реализовано.
        public override SortedDictionary<int, long> GetCycles(int lowBound, int hightBound)
        {
            log.Info("Getting cycles.");
            cyclesCounter = new CyclesCounter(container);
            SortedDictionary<int, long> cyclesCount = new SortedDictionary<int, long>();
            long count = 0;
            for (int i = lowBound; i <= hightBound; i++)
            {
                count = cyclesCounter.getCyclesCount(i);
                cyclesCount.Add(i, count);
            }

            return cyclesCount;
        }

        // Возвращается распределение чисел мотивов. Реализовано.
        public override SortedDictionary<int, float> GetMotivs(int lowBound, int hightBound)
        {
            log.Info("Getting motifs.");

            var motivfinder = new MotifFinder();
            var motifisCount = new SortedDictionary<int, float>();
            var motifisCountResult = new SortedDictionary<int, float>();
            Graph graph = Graph.reformatToOurGraghFromBAContainer(container.Neighbourship);
            for (int motifDegree = lowBound; motifDegree <= hightBound; motifDegree++)
            {
                motivfinder.SearchMotifs(graph, motifDegree);
                motifisCount = motivfinder.dictionaryIdsValues();
                foreach (var key in motifisCount.Keys)
                    motifisCountResult.Add(key, motifisCount[key]);
            }

            return motifisCountResult;
        }

        // Возвращает распределение триугольников, прикрепленных к вершине.
        public override SortedDictionary<int, int> GetTrianglesDistribution()
        {
            log.Info("Getting triangles distribution.");

            if (-1 == avgPath)
            {
                CountEssentialOptions();
            }

            var trianglesDistribution = new SortedDictionary<int, int>();
            for (int i = 0; i < container.Size; ++i)
            {
                var countTringle = (int)edgesBetweenNeighbours[i];
                if (trianglesDistribution.ContainsKey(countTringle))
                {
                    trianglesDistribution[countTringle]++;
                }
                else
                {
                    trianglesDistribution.Add(countTringle, 1);
                }
            }

            return trianglesDistribution;
        }

        // Возвращается распределение чисел  связанных подграфов в графе.
        public override SortedDictionary<int, int> GetConnSubGraph()
        {
            var connectedSubGraphDic = new SortedDictionary<int, int>();
            Queue<int> q = new Queue<int>();
            var nodes = new Node[container.Size];
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = new Node();
            var list = new List<int>();

            for (int i = 0; i < container.Size; i++)
            {
                int order = 0;
                q.Enqueue(i);
                while (q.Count != 0)
                {
                    var item = q.Dequeue();
                    if (nodes[item].lenght != 2)
                    {
                        if (nodes[item].lenght == -1)
                        {
                            order++;
                        }
                        list = container.Neighbourship[item];
                        nodes[item].lenght = 2;

                        for (int j = 0; j < list.Count; j++)
                        {
                            if (nodes[list[j]].lenght == -1)
                            {
                                nodes[list[j]].lenght = 1;
                                order++;
                                q.Enqueue(list[j]);
                            }

                        }
                    }
                }

                if (order != 0)
                {
                    if (connectedSubGraphDic.ContainsKey(order))
                        connectedSubGraphDic[order]++;
                    else
                        connectedSubGraphDic.Add(order, 1);
                }
            }
            return connectedSubGraphDic;
        }

        // Закрытая часть класса (не из общего интерфейса). //

        private List<double> edgesBetweenNeighbours;

        private double avgPath = -1;
        private int diameter = -1;
        private SortedDictionary<int, int> pathDistribution = new SortedDictionary<int, int>();
        private List<SortedList<int, int>> cycles4 = new List<SortedList<int, int>>();
        private CyclesCounter cyclesCounter;

        private void Initialization()
        {
            edgesBetweenNeighbours = new List<double>();
            for (int i = 0; i < container.Size; ++i)
                edgesBetweenNeighbours.Add(-1);
            cyclesCounter = new CyclesCounter(container);
        }

        // Внутренный тип для работы BFS алгоритма.
        private class Node
        {
            public int ancestor = -1;
            public int lenght = -1;
            public int m_4Cycles = 0;
            public Node() { }
        }

        // Реализация BFS алгоритма.
        private void BFS(int i, Node[] nodes)
        {
            nodes[i].lenght = 0;
            nodes[i].ancestor = 0;
            bool b = true;
            Queue<int> q = new Queue<int>();
            q.Enqueue(i);
            int u;
            if (edgesBetweenNeighbours[i] == -1)
                edgesBetweenNeighbours[i] = 0;
            else
                b = false;

            while (q.Count != 0)
            {
                u = q.Dequeue();
                List<int> l = container.Neighbourship[u];
                for (int j = 0; j < l.Count; ++j)
                    if (nodes[l[j]].lenght == -1)
                    {
                        nodes[l[j]].lenght = nodes[u].lenght + 1;
                        nodes[l[j]].ancestor = u;
                        q.Enqueue(l[j]);
                    }
                    else
                    {
                        if (nodes[u].lenght == 1 && nodes[l[j]].lenght == 1 && b)
                        {
                            ++edgesBetweenNeighbours[i];
                        }
                    }
            }
            if (b)
                edgesBetweenNeighbours[i] /= 2;
        }

        // Выполняет подсчет сразу 3 свойств - средняя длина пути, диаметр и пути между вершинами.
        // Нужно вызвать перед получением этих свойств не изнутри.
        private void CountEssentialOptions()
        {
            if (edgesBetweenNeighbours.Count == 0)
            {
                for (int i = 0; i < container.Size; ++i)
                    edgesBetweenNeighbours.Add(-1);
            }
            double avg = 0;
            int diametr = 0, k = 0;

            for (int i = 0; i < container.Size; ++i)
            {
                for (int j = i + 1; j < container.Size; ++j)
                {
                    int way = MinimumWay(i, j);
                    if (way == -1)
                        continue;
                    if (pathDistribution.ContainsKey(way))
                        pathDistribution[way]++;
                    else
                        pathDistribution.Add(way, 1);

                    if (way > diametr)
                        diametr = way;

                    avg += way;
                    ++k;
                }
            }
            Node[] nodes = new Node[container.Size];
            for (int t = 0; t < container.Size; ++t)
                nodes[t] = new Node();

            BFS(container.Size - 1, nodes);
            avg /= k;

            avgPath = avg;
            diameter = diametr;
        }

        // Возвращает число циклов 4, которые содержат данную вершину.
        private int CalculatCycles4(int i)
        {
            Node[] nodes = new Node[container.Size];
            for (int k = 0; k < container.Size; ++k)
                nodes[k] = new Node();
            int cyclesOfOrderi4 = 0;
            nodes[i].lenght = 0;
            nodes[i].ancestor = 0;
            Queue<int> q = new Queue<int>();
            q.Enqueue(i);
            int u;

            while (q.Count != 0)
            {
                u = q.Dequeue();
                List<int> l = container.Neighbourship[u];
                for (int j = 0; j < l.Count; ++j)
                    if (nodes[l[j]].lenght == -1)
                    {
                        nodes[l[j]].lenght = nodes[u].lenght + 1;
                        nodes[l[j]].ancestor = u;
                        q.Enqueue(l[j]);
                    }
                    else
                    {
                        if (nodes[u].lenght == 2 && nodes[l[j]].lenght == 1 && nodes[u].ancestor != l[j])
                        {
                            SortedList<int, int> cycles4I = new SortedList<int, int>();
                            cyclesOfOrderi4++;
                        }
                    }
            }

            return cyclesOfOrderi4;
        }

        // Возвращает распределение степеней.
        private SortedDictionary<int, int> DegreeDistribution()
        {
            SortedDictionary<int, int> degreeDistribution = new SortedDictionary<int, int>();
            for (int i = 0; i < container.Size; ++i)
                degreeDistribution[i] = new int();
            for (int i = 0; i < container.Size; ++i)
            {
                int degreeOfVertexI = container.Neighbourship[i].Count;
                degreeDistribution[degreeOfVertexI]++;
            }
            for (int i = 0; i < container.Size; i++)
                if (degreeDistribution[i] == 0)
                    degreeDistribution.Remove(i);

            return degreeDistribution;
        }

        private int fullSubGgraph(int u)    // Разобраться, почему не реализована соответсвующая функция интерфейса.
        {
            List<int> n1;
            n1 = container.Neighbourship[u];
            List<int> n2 = new List<int>();
            int l = 0;
            bool t;
            int k = 0;
            while (l != n1.Count)
            {
                n2.Clear();
                n2.Add(u);
                if (l != 0)
                    n2.Add(n1[l]);
                for (int i = 0; i < n1.Count && i != l; i++)
                {
                    t = true;
                    for (int j = 0; j < n2.Count; j++)
                        if (container.AreNeighbours(n1[i], n2[j]) == false)
                        {
                            t = false;
                            break;

                        }
                    if (t == true)
                        n2.Add(n1[i]);
                }
                int p = n2.Count;
                if (p > k)
                    k = p;
                l++;
            }
            return k;
        }

        // Возвращает длину минимальной пути между данными вершинами.
        private int MinimumWay(int i, int j)
        {
            if (i == j)
                return 0;

            Node[] nodes = new Node[container.Size];
            for (int k = 0; k < container.Size; ++k)
                nodes[k] = new Node();

            BFS(i, nodes);

            return nodes[j].lenght;
        }

        // Возвращает степень максимального соединенного подграфа. Не используется.
        private int GetMaxFullSubgraph()
        {
            int k = 0;
            for (int i = 0; i < container.Size; i++)
                if (this.fullSubGgraph(i) > k)
                    k = this.fullSubGgraph(i);

            return k;
        }

        private long Get4OrderCyclesOfNode(int j)
        {
            List<int> neigboursList = container.Neighbourship[j];
            List<int> neigboursList1 = new List<int>();
            List<int> neigboursList2 = new List<int>();
            long count = 0;
            for (int i = 0; i < neigboursList.Count; i++)
            {
                neigboursList1 = container.Neighbourship[neigboursList[i]];
                for (int t = 0; t < neigboursList1.Count; t++)
                {
                    if (j != neigboursList1[t])
                    {
                        neigboursList2 = container.Neighbourship[neigboursList1[t]];
                        for (int k = 0; k < neigboursList2.Count; k++)
                            if (container.AreNeighbours(neigboursList2[k], j) && neigboursList2[k] != neigboursList1[t] && neigboursList2[k] != neigboursList[i])
                                count++;
                    }
                }


            }

            return count / 2;

        }
    }
}