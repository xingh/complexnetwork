﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Algorithms;
using CommonLibrary.Model;
using log4net;

namespace Model.HierarchicModel.Realization
{
    // Реализация анализатора (Block-Hierarchic).
    public class HierarchicAnalyzer : AbstarctGraphAnalyzer
    {
        // Организация работы с лог файлом.
        protected new static readonly ILog log = LogManager.GetLogger(typeof(HierarchicAnalyzer));
        private readonly SortedDictionary<int, int> pathDistribution = new SortedDictionary<int, int>();
        private double avgPath = -1;

        // Контейнер, в котором содержится граф конкретной модели (Block-Hierarchic).
        private HierarchicContainer container;
        private int diameter = -1;
        private ArrayList eigenValues;

        public HierarchicAnalyzer(HierarchicContainer c)
        {
            log.Info("Creating HierarchicAnalyzer object.");
            container = c;
        }

        // Контейнер, в котором содержится сгенерированный граф (полученный от генератора).
        public override AbstractGraphContainer Container
        {
            get { return container; }
            set { container = (HierarchicContainer)value; }
        }

        // Возвращается средняя длина пути в графе. Реализовано.
        public override double GetAveragePath()
        {
            log.Info("Getting average path length.");

            if (-1 == avgPath)
            {
                CountPathDistribution();
            }

            return Math.Round(avgPath, 14);

            //long[] pathsInfo = GetSubgraphsPathInfo(0, 0);
            // !petq e bajanel chanaparhneri qanaki vra!
            //return 2 * (pathsInfo[0] + pathsInfo[2]) / ((double)container.Size *
            //    ((double)container.Size - 1));
        }

        // Возвращается диаметр графа. Реализовано.
        public override int GetDiameter()
        {
            log.Info("Getting diameter.");

            if (-1 == diameter)
            {
                CountPathDistribution();
            }

            return diameter;
        }

        // Возвращается число циклов длиной 3 в графе. Реализовано.
        public override long GetCycles3()
        {
            log.Info("Getting count of cycles - order 3.");
            return (long)Count3Cycle(0, 0)[0];
        }

        // Возвращается число циклов длиной 4 в графе. Реализовано.
        public override long GetCycles4()
        {
            log.Info("Getting count of cycles - order 4.");
            return (long)Count4Cycle(0, 0)[0];
        }

        public override long GetCycles5()
        {
            log.Info("Getting count of cycles - order 5.");
            return (long)Count5Cycle(0, 0)[0];
        }

        // Возвращается число циклов длиной 3 в графе, с помощью собственных значений. Реализовано.
        // Используется только для модели ParisiHierarchicModel.
        public override int GetCyclesEigen3()
        {
            log.Info("Getting count of cycles by eigen values - order 3.");
            return (int)CalcCyclesCount(3);
        }

        // Возвращается число циклов длиной 4 в графе. Реализовано.
        // Используется только для модели ParisiHierarchicModel.
        public override int GetCyclesEigen4()
        {
            log.Info("Getting count of cycles by eigen values - order 4.");
            return (int)CalcCyclesCount(4);
        }

        // Возвращается массив собственных значений матрицы смежности. Реализовано.
        public override ArrayList GetEigenValues()
        {
            log.Info("Getting eigen values array.");
            bool[,] m = container.GetMatrix();
            var eg = new EigenValueUtils();
            try
            {
                eigenValues = eg.CalculateEigenValue(m);
                return eigenValues;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return new ArrayList();
            }

            // Правильно для модели ParisiHierarchicModel.
            //return new ArrayList(CalcEigenValue(container.TreeVector(), container.BranchIndex));
        }

        // Возвращается распределение длин между собственными значениями. Реализовано
        public override SortedDictionary<double, int> GetDistEigenPath()
        {
            log.Info("Getting distances between eigen values.");
            bool[,] m = container.GetMatrix();
            var eg = new EigenValueUtils();
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

        // Возвращается распределение длин минимальных путей в графе. Реализовано.
        public override SortedDictionary<int, int> GetMinPathDist()
        {
            log.Info("Getting minimal distances between vertices.");

            if (-1 == avgPath)
            {
                CountPathDistribution();
            }

            return pathDistribution;
        }

        // Возвращается степенное распределение графа. Реализовано.
        public override SortedDictionary<int, int> GetDegreeDistribution()
        {
            log.Info("Getting degree distribution.");
            return ArrayCntAdjacentCntVertexes(0, 0);
        }

        // Возвращается распределение коэффициентов кластеризации графа. Реализовано.
        public override SortedDictionary<double, int> GetClusteringCoefficient()
        {
            log.Info("Getting clustering coefficients.");
            var result = new SortedDictionary<double, int>();

            for (int i = 0; i < container.Size; ++i)
            {
                double dresult = Math.Round(ClusterringCoefficientOfVertex(i), 4);
                if (result.Keys.Contains(dresult))
                    ++result[dresult];
                else
                    result.Add(dresult, 1);
            }

            return result;
        }

        // Возвращается распределение чисел связанных подграфов в графе. Реализовано.
        public override SortedDictionary<int, int> GetConnSubGraph()
        {
            log.Info("Getting connected subgraphs.");
            return AmountConnectedSubGraphs(0, 0);
        }

        // ???????? !Исправить! подумать о включении в общий интерфейс
        public SortedDictionary<int, int> GetConnSubGraphPerLevel(Int16 currentLevel)
        {
            int currentLevelInTree = container.Level - currentLevel;
            var result = new SortedDictionary<int, int>();

            for (int i = 0; i < Math.Pow(container.BranchIndex, currentLevelInTree); ++i)
            {
                SortedDictionary<int, int> res =
                    AmountConnectedSubGraphsPerLevel(i, currentLevelInTree);

                foreach (var kvt in res)
                {
                    if (result.Keys.Contains(kvt.Key))
                        result[kvt.Key] += kvt.Value;
                    else
                        result.Add(kvt.Key, kvt.Value);
                }
            }

            return result;
        }

        // ?????????????????????????
        public double GetAvgConnSubGraphPerLevel(Int16 currentLevel)
        {
            int currentLevelInTree = container.Level - currentLevel;
            double result = 0;

            double countOfClusters = Math.Pow(container.BranchIndex, currentLevelInTree);
            for (int i = 0; i < countOfClusters; ++i)
            {
                SortedDictionary<int, int> res =
                    AmountConnectedSubGraphsPerLevel(i, currentLevelInTree);
                double maxRes = (res.Count() == 0) ? 1 : res.Last().Key;
                result += maxRes;
            }

            result /= countOfClusters;
            return result;
        }

        // ???????? !Исправить! подумать о включении в общий интерфейс
        public SortedDictionary<int, int> GetConnSubGraphSublevels(Int16 currentLevel)
        {
            int currentLevelInTree = container.Level - currentLevel;
            var result = new SortedDictionary<int, int>();

            for (int i = 0; i < Math.Pow(container.BranchIndex, currentLevelInTree); ++i)
            {
                SortedDictionary<int, int> res =
                    AmountConnectedSubGraphs(i, currentLevelInTree);

                foreach (var kvt in res)
                {
                    if (result.Keys.Contains(kvt.Key))
                        result[kvt.Key] += kvt.Value;
                    else
                        result.Add(kvt.Key, kvt.Value);
                }
            }

            return result;
        }

        // Возвращается распределение чисел циклов. Реализовано.
        public override SortedDictionary<int, long> GetCycles(int lowBound, int hightBound)
        {
            log.Info("Getting cycles.");
            var engForCycles = new EngineForCycles(container);

            var result = new SortedDictionary<int, long>();
            string infoStr = "Getting cycles of order ";
            for (int l = lowBound; l <= hightBound; ++l)
            {
                infoStr += l + ".";
                log.Info(infoStr);
                result.Add(l, (int)engForCycles.GetCycleCount(l));
            }

            return result;
        }


        // Возвращает распределение триугольников, прикрепленных к вершине.
        public override SortedDictionary<int, int> GetTrianglesDistribution()
        {
            log.Info("Getting triangles distribution.");

            var result = new SortedDictionary<int, int>();
            for (int i = 0; i < container.Size; ++i)
            {
                var triangleCountOfVertex = (int)Count3CycleOfVertex(i, 0)[0];
                if (result.Keys.Contains(triangleCountOfVertex))
                    ++result[triangleCountOfVertex];
                else
                    result.Add(triangleCountOfVertex, 1);
            }

            return result;
        }

        // Закрытая часть класса (не из общего интерфейса). //

        private void CountPathDistribution()
        {
            double avgPath = 0;
            int diameter = 0, countOfWays = 0;

            for (int i = 0; i < container.Size; ++i)
            {
                for (int j = i + 1; j < container.Size; ++j)
                {
                    int way = container.MinimumWay(i, j);
                    if (way == -1)
                        continue;
                    if (pathDistribution.ContainsKey(way))
                        pathDistribution[way]++;
                    else
                        pathDistribution.Add(way, 1);

                    if (way > diameter)
                        diameter = way;

                    avgPath += way;
                    ++countOfWays;
                }
            }

            this.avgPath = avgPath / countOfWays;
            this.diameter = diameter;
        }

        // Возвращает степень данного узла на данном уровне (в соответствующем кластере).
        private double VertexDegree(int vertexNumber, int level)
        {
            double result = 0;
            int vertexIndex = 0, nodeNumber = 0;
            for (int i = container.Level - 1; i >= level; --i)
            {
                vertexIndex = container.TreeIndex(vertexNumber, i + 1) % container.BranchIndex;
                nodeNumber = container.TreeIndex(vertexNumber, i);
                BitArray node = container.TreeNode(i, nodeNumber);
                result += container.Links(vertexIndex, nodeNumber, i) *
                          Math.Pow(container.BranchIndex, container.Level - i - 1);
            }

            return result;
        }

        // Возвращает распределение степеней.
        // Распределение степеней вычисляется в данном узле данного уровня.
        private SortedDictionary<int, int> ArrayCntAdjacentCntVertexes(int numberNode, int level)
        {
            if (level == container.Level)
            {
                var returned = new SortedDictionary<int, int>();
                returned[0] = 1;
                return returned;
            }
            BitArray node = container.TreeNode(level, numberNode);

            var arraysReturned = new SortedDictionary<int, int>();
            var array = new SortedDictionary<int, int>();
            int powPK = Convert.ToInt32(Math.Pow(container.BranchIndex, container.Level - level - 1));

            for (int i = numberNode * container.BranchIndex; i < container.BranchIndex * (numberNode + 1); i++)
            {
                int nodeNumberi = i - numberNode * container.BranchIndex;
                array = ArrayCntAdjacentCntVertexes(i, level + 1);
                int countAjacentsThisnode = container.CountConnectedBlocks(node, nodeNumberi);
                foreach (var kvt in array)
                {
                    int key = kvt.Key + countAjacentsThisnode * powPK;
                    if (arraysReturned.ContainsKey(key))
                        arraysReturned[key] += kvt.Value;
                    else
                        arraysReturned.Add(key, kvt.Value);
                }
            }
            return arraysReturned;
        }

        // Возвращает информацию о пути подграфа (реализована рекурсивным образом).
        // Используется алгоритм Флойда для вычисления минимальных путей между вершинами графа.
        private long[] GetSubgraphsPathInfo(int level, long nodeNumber)
        {
            //resultArr's and tempinfo's 
            //1 element is current paths, that can't minimized, lengths sum
            //2 temp paths count, that have chance to be minimized
            //3 >2 paths' lengths sum
            long[] resultArr = { 0, 0, 0 };
            long[] tempInfo = { 0, 0, 0 };

            // Если это не лист дерева, то проход по всем дочерным узлам (рекурсивный вызов).
            if (level < container.Level - 1)
            {
                for (int i = 0; i < container.BranchIndex; i++)
                {
                    tempInfo = GetSubgraphsPathInfo(level + 1, nodeNumber * container.BranchIndex + i);

                    resultArr[0] += tempInfo[0];
                    if (container.NodeChildAdjacentsCount(level, nodeNumber, i) > 0)
                    {
                        resultArr[0] += tempInfo[1] * 2;
                    }
                    else
                    {
                        resultArr[1] += tempInfo[1];
                        resultArr[2] += tempInfo[2];
                    }
                }
            }

            // Получение суммы длин минимальных путей (и дополнительной информации) для данного узла.
            tempInfo = Engine.FloydMinPath(container.NodeMatrix(level, nodeNumber));

            double tempPow = Math.Pow(container.BranchIndex, container.Level - level - 1);
            resultArr[0] += tempInfo[0] * Convert.ToInt64(Math.Pow(tempPow, 2));
            resultArr[1] += tempInfo[1] * Convert.ToInt64(Math.Pow(tempPow, 2));
            resultArr[2] += tempInfo[2] * Convert.ToInt64(Math.Pow(tempPow, 2));

            return resultArr;
        }

        private SortedDictionary<int, int> AmountConnectedSubGraphs(int numberNode, int level)
        {
            var retArray = new SortedDictionary<int, int>();

            if (level == container.Level)
            {
                retArray[1] = 1;
                return retArray;
            }
            BitArray node = container.TreeNode(level, numberNode);

            bool haveOne = false;
            for (int i = 0; i < container.BranchIndex; i++)
            {
                if (container.CountConnectedBlocks(node, i) == 0)
                {
                    SortedDictionary<int, int> array = AmountConnectedSubGraphs(numberNode * container.BranchIndex + i,
                        level + 1);

                    foreach (var kvt in array)
                    {
                        if (retArray.Keys.Contains(kvt.Key))
                            retArray[kvt.Key] += kvt.Value;
                        else
                            retArray.Add(kvt.Key, kvt.Value);
                    }
                }
                else
                    haveOne = true;
            }

            if (haveOne)
            {
                int powPK = Convert.ToInt32(Math.Pow(container.BranchIndex, container.Level - level - 1));
                var engForConnectedComponent = new EngineForConnectedComp();
                ArrayList arrConnComp = engForConnectedComponent.GetCountConnSGruph(container.nodeMatrixList(node),
                    container.BranchIndex);
                for (int i = 0; i < arrConnComp.Count; i++)
                {
                    var countConnCompi = (int)arrConnComp[i];
                    if (retArray.Keys.Contains(countConnCompi * powPK))
                        retArray[countConnCompi * powPK] += 1;
                    else
                        retArray.Add(countConnCompi * powPK, 1);
                }
            }

            return retArray;
        }

        // ???????????????
        private SortedDictionary<int, int> AmountConnectedSubGraphsPerLevel(int numberNode,
            int level)
        {
            var retArray = new SortedDictionary<int, int>();

            if (level == container.Level)
            {
                retArray[1] = 1;
                return retArray;
            }
            BitArray node = container.TreeNode(level, numberNode);

            int powPK = Convert.ToInt32(Math.Pow(container.BranchIndex,
                container.Level - level - 1));
            var engForConnectedComponent = new EngineForConnectedComp();
            ArrayList arrConnComp =
                engForConnectedComponent.GetCountConnSGruph(container.nodeMatrixList(node),
                    container.BranchIndex);
            for (int i = 0; i < arrConnComp.Count; i++)
            {
                var countConnCompi = (int)arrConnComp[i];
                if (retArray.Keys.Contains(countConnCompi * powPK))
                    retArray[countConnCompi * powPK] += 1;
                else
                    retArray.Add(countConnCompi * powPK, 1);
            }

            return retArray;
        }

        // Возвращает число циклов порядка 3 в нулевом элементе SortedDictionary<int, double>.
        // Число циклов вычисляется в данном узле данного уровня.
        private SortedDictionary<int, double> Count3Cycle(int numberNode, int level)
        {
            var retArray = new SortedDictionary<int, double>();
            retArray[0] = 0; // count cycles
            retArray[1] = 0; // count edges

            if (level == container.Level)
            {
                return retArray;
            }
            double countCycle = 0;
            var countEdge = new double[container.BranchIndex];
            int countOne = 0;
            double powPK = Math.Pow(container.BranchIndex, container.Level - level - 1);
            BitArray node = container.TreeNode(level, numberNode);

            for (int i = numberNode * container.BranchIndex; i < container.BranchIndex * (numberNode + 1); i++)
            {
                var arr = new SortedDictionary<int, double>();
                arr = Count3Cycle(i, level + 1);
                countEdge[i - numberNode * container.BranchIndex] = arr[1];
                retArray[0] += arr[0];
                retArray[1] += arr[1];
            }
            for (int i = 0; i < (container.BranchIndex * (container.BranchIndex - 1) / 2); i++)
            {
                countOne += (node[i]) ? 1 : 0;
            }
            retArray[1] += countOne * powPK * powPK;


            for (int i = numberNode * container.BranchIndex; i < container.BranchIndex * (numberNode + 1); i++)
            {
                for (int j = (i + 1); j < container.BranchIndex * (numberNode + 1); j++)
                {
                    if (container.IsConnectedTwoBlocks(node, i - numberNode * container.BranchIndex,
                        j - numberNode * container.BranchIndex))
                    {
                        countCycle += (countEdge[i - numberNode * container.BranchIndex] +
                                       countEdge[j - numberNode * container.BranchIndex]) * powPK;

                        for (int k = (j + 1); k < container.BranchIndex * (numberNode + 1); k++)
                        {
                            if (container.IsConnectedTwoBlocks(node, j - numberNode * container.BranchIndex,
                                k - numberNode * container.BranchIndex)
                                && container.IsConnectedTwoBlocks(node, i - numberNode * container.BranchIndex,
                                    k - numberNode * container.BranchIndex))
                                countCycle += powPK * powPK * powPK;
                        }
                    }
                }
            }
            retArray[0] += countCycle;

            return retArray;
        }

        // Возвращает число циклов порядка 4 в нулевом элементе SortedDictionary<int, double>.
        // Число циклов вычисляется в данном узле данного уровня.
        private SortedDictionary<int, double> Count4Cycle(int nodeNumber, int level)
        {
            var arrayReturned = new SortedDictionary<int, double>();
            arrayReturned[0] = 0; // число циклов порядка 4
            arrayReturned[1] = 0; // число путей длиной 1 (ребер)
            arrayReturned[2] = 0; // число путей длиной 2

            if (level == container.Level)
            {
                return arrayReturned;
            }
            var array =
                new SortedDictionary<int, SortedDictionary<int, double>>();
            int bIndex = container.BranchIndex;

            for (int i = nodeNumber * bIndex; i < (nodeNumber + 1) * bIndex; ++i)
            {
                array[i] = Count4Cycle(i, level + 1);
                arrayReturned[0] += array[i][0];
                arrayReturned[1] += array[i][1];
                arrayReturned[2] += array[i][2];
            }

            BitArray node = container.TreeNode(level, nodeNumber);
            double powPK = Math.Pow(container.BranchIndex, container.Level - level - 1);

            for (int i = nodeNumber * bIndex; i < (nodeNumber + 1) * bIndex; ++i)
            {
                for (int j = i + 1; j < (nodeNumber + 1) * bIndex; ++j)
                {
                    if (container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, j - nodeNumber * bIndex))
                    {
                        arrayReturned[0] += (array[i][2] + array[j][2]) * powPK;
                        arrayReturned[0] += 2 * array[i][1] * array[j][1];

                        arrayReturned[0] += Math.Pow(powPK * (powPK - 1) / 2, 2);

                        arrayReturned[1] += powPK * powPK;

                        arrayReturned[2] += 2 * powPK * (array[i][1] + array[j][1]);
                        arrayReturned[2] += powPK * powPK * (powPK - 1);
                    }

                    for (int k = j + 1; k < (nodeNumber + 1) * bIndex; ++k)
                    {
                        if (container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, j - nodeNumber * bIndex) &&
                            container.IsConnectedTwoBlocks(node, j - nodeNumber * bIndex, k - nodeNumber * bIndex) &&
                            container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, k - nodeNumber * bIndex))
                        {
                            arrayReturned[0] += 2 * (array[i][1] + array[j][1] + array[k][1]) * powPK * powPK;
                        }

                        if (container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, j - nodeNumber * bIndex) &&
                            container.IsConnectedTwoBlocks(node, j - nodeNumber * bIndex, k - nodeNumber * bIndex))
                        {
                            arrayReturned[0] += powPK * powPK * powPK * (powPK - 1) / 2;

                            arrayReturned[2] += powPK * powPK * powPK;
                        }

                        if (container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, k - nodeNumber * bIndex) &&
                            container.IsConnectedTwoBlocks(node, k - nodeNumber * bIndex, j - nodeNumber * bIndex))
                        {
                            arrayReturned[0] += powPK * powPK * powPK * (powPK - 1) / 2;

                            arrayReturned[2] += powPK * powPK * powPK;
                        }

                        if (container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, j - nodeNumber * bIndex) &&
                            container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, k - nodeNumber * bIndex))
                        {
                            arrayReturned[0] += powPK * powPK * powPK * (powPK - 1) / 2;

                            arrayReturned[2] += powPK * powPK * powPK;
                        }

                        for (int l = k + 1; l < (nodeNumber + 1) * bIndex; ++l)
                        {
                            bool b1 =
                                container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, j - nodeNumber * bIndex) &&
                                container.IsConnectedTwoBlocks(node, j - nodeNumber * bIndex, k - nodeNumber * bIndex) &&
                                container.IsConnectedTwoBlocks(node, k - nodeNumber * bIndex, l - nodeNumber * bIndex) &&
                                container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, l - nodeNumber * bIndex);
                            bool b2 =
                                container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, j - nodeNumber * bIndex) &&
                                container.IsConnectedTwoBlocks(node, j - nodeNumber * bIndex, l - nodeNumber * bIndex) &&
                                container.IsConnectedTwoBlocks(node, l - nodeNumber * bIndex, k - nodeNumber * bIndex) &&
                                container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, k - nodeNumber * bIndex);
                            bool b3 =
                                container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, l - nodeNumber * bIndex) &&
                                container.IsConnectedTwoBlocks(node, l - nodeNumber * bIndex, j - nodeNumber * bIndex) &&
                                container.IsConnectedTwoBlocks(node, j - nodeNumber * bIndex, k - nodeNumber * bIndex) &&
                                container.IsConnectedTwoBlocks(node, i - nodeNumber * bIndex, k - nodeNumber * bIndex);
                            if (b1)
                            {
                                arrayReturned[0] += powPK * powPK * powPK * powPK;
                            }

                            if (b2)
                            {
                                arrayReturned[0] += powPK * powPK * powPK * powPK;
                            }

                            if (b3)
                            {
                                arrayReturned[0] += powPK * powPK * powPK * powPK;
                            }
                        }
                    }
                }
            }

            return arrayReturned;
        }

        // Возвращает коэффициент класстеризации для данной вершины (vertexNumber).
        // Вычисляется с помощью числа циклов порядка 3, прикрепленных к данной вершине.
        private double ClusterringCoefficientOfVertex(int vertexNumber)
        {
            double degree = VertexDegree(vertexNumber, 0);
            if (degree == 0 || degree == 1)
                return 0;
            return (2 * Count3CycleOfVertex(vertexNumber, 0)[0]) / (degree * (degree - 1));
        }

        // Возвращает число циклов порядка 3 прикрепленных к данному узлу 
        // в нулевом элементе SortedDictionary<int, double>.
        // Число циклов вычисляется в данном узле данного уровня.
        private SortedDictionary<int, double> Count3CycleOfVertex(int vertexNumber, int level)
        {
            var result = new SortedDictionary<int, double>();
            result[0] = 0; // число циклов 3 прикрепленных к данному узлу
            result[1] = 0; // число ребер в данном подграфе (такое вычисление повышает эффективность)

            if (level == container.Level)
            {
                return result;
            }
            int numberNode = container.TreeIndex(vertexNumber, level);
            int vertexIndex = container.TreeIndex(vertexNumber, level + 1) % container.BranchIndex;
            BitArray node = container.TreeNode(level, numberNode);
            double powPK = Math.Pow(container.BranchIndex, container.Level - level - 1);

            SortedDictionary<int, double> previousResult = Count3CycleOfVertex(vertexNumber, level + 1);
            result[0] += previousResult[0];
            result[1] += previousResult[1];

            double degree = VertexDegree(vertexNumber, level + 1);
            for (int j = numberNode * container.BranchIndex; j < container.BranchIndex * (numberNode + 1); ++j)
            {
                if (container.IsConnectedTwoBlocks(node, vertexIndex, j - numberNode * container.BranchIndex))
                {
                    result[0] += container.CountEdges(j, level + 1); //- numberNode * container.BranchIndex
                    result[0] += powPK * degree;

                    for (int k = j + 1; k < container.BranchIndex * (numberNode + 1); ++k)
                    {
                        if (container.IsConnectedTwoBlocks(node, j - numberNode * container.BranchIndex,
                            k - numberNode * container.BranchIndex) &&
                            container.IsConnectedTwoBlocks(node, k - numberNode * container.BranchIndex,
                                vertexIndex))
                        {
                            result[0] += powPK * powPK;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Возвращает коэффициент кластеризации графа.
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private double ClusteringCoefficientOfVertex(long vert)
        {
            double sum = 0;
            long adjCount = 0;
            //loop over all levels
            for (int level = container.Level - 1; level >= 0; level--)
            {
                //get vertex position in current level
                long vertNodeNum = Convert.ToInt64(Math.Floor(Convert.ToDouble(vert / container.BranchIndex)));
                int vertNodeInd = Convert.ToInt32(vert % container.BranchIndex);

                //get vertex adjacent vertexes in current node
                List<int> adjIndexes = container.NodeChildAdjacentsArray(level, vertNodeNum, vertNodeInd);

                long levelVertexCount = Convert.ToInt64(Math.Pow(container.BranchIndex, container.Level - level - 1));
                //vertex subtree vertexes with adjacent subtrees vertexes
                long vertexSubTreeWithAdjSubTrees = adjCount * levelVertexCount * adjIndexes.Count;
                sum += vertexSubTreeWithAdjSubTrees;
                //add adjacent vertexes count
                adjCount += levelVertexCount * adjIndexes.Count;
                //adjacent subtrees weights
                for (int i = 0; i < container.BranchIndex; i++)
                {
                    if (adjIndexes.IndexOf(i) != -1)
                    {
                        sum += container.CountEdges(vertNodeNum * container.BranchIndex + i, level + 1);
                    }
                }
                //connectivity of adjacent subtrees
                for (int i = 0; i < container.BranchIndex; i++)
                {
                    if (adjIndexes.IndexOf(i) != -1)
                    {
                        for (int j = i; j < container.BranchIndex; j++)
                        {
                            if (i != j && i != vertNodeInd && j != vertNodeInd)
                            {
                                sum += container.AreAdjacent(level, vertNodeNum, i, j) * Math.Pow(levelVertexCount, 2);
                            }
                        }
                    }
                }

                vert = vertNodeNum;
            }
            double vertClustCoef = 0;
            if (adjCount > 1)
            {
                vertClustCoef = 2 * sum / (adjCount * (adjCount - 1));
            }
            else if (adjCount == 1)
            {
                vertClustCoef = sum;
            }

            return vertClustCoef;
        }

        /// <summary>
        ///     Возвращает собственные значения.
        /// </summary>
        /// <param name="bitArr"></param>
        /// <param name="mBase"></param>
        /// <param name="EigValue"></param>
        private List<double> CalcEigenValue(BitArray bitArr, int mBase)
        {
            var EigValue = new List<double>();

            var basicEigValue = new List<double>(mBase);
            var eigValueE = new List<double>(mBase);
            for (int i = 1; i < mBase; ++i)
            {
                eigValueE.Add(0);
            }
            eigValueE.Add(mBase);
            int bitArrSize = bitArr.Count;
            if (bitArr[0] == false)
            {
                if (bitArr[1] == false)
                {
                    for (int i = 0; i < mBase; ++i)
                    {
                        basicEigValue.Add(0);
                    }
                }
                else
                {
                    for (int i = 0; i < mBase; ++i)
                    {
                        basicEigValue.Add(1);
                    }
                }
            }
            else
            {
                if (bitArr[1] == false)
                {
                    for (int i = 1; i < mBase; ++i)
                    {
                        basicEigValue.Add(-1);
                    }
                    basicEigValue.Add(mBase - 1);
                }
                else
                {
                    for (int i = 1; i < mBase; ++i)
                    {
                        basicEigValue.Add(0);
                    }
                    basicEigValue.Add(mBase);
                }
            }
            int size = mBase;
            var BA = new BitArray(bitArrSize + 1);
            for (int i = 0; i < bitArrSize; ++i)
                BA[i] = bitArr[i];
            BA.Set(bitArrSize, false);
            int x = 1;
            while (x != bitArrSize)
            {
                foreach (int elemE in eigValueE)
                {
                    int t1, t2;
                    if (BA[x])
                        t1 = 1;
                    else
                        t1 = 0;
                    if (BA[x + 1])
                        t2 = 1;
                    else
                        t2 = 0;
                    foreach (int elem in basicEigValue)
                        EigValue.Add(elem * elemE - t1 + t2);
                }
                ++x;
                basicEigValue.Clear();
                basicEigValue.InsertRange(0, EigValue);
                EigValue.Clear();
            }

            EigValue.InsertRange(0, basicEigValue);
            return EigValue;
        }

        // Возвращает число циклов данного порядка, с помощью собственных значений.
        public double CalcCyclesCount(int cycleLength)
        {
            List<double> eigValue = CalcEigenValue(container.TreeVector(), container.BranchIndex);

            double total = 0;
            foreach (int i in eigValue)
            {
                total += Math.Pow(i, cycleLength);
            }
            return total / (2 * cycleLength);
        }

        // Возвращает среднее степеней. Не используется.
        public double AverageDegree()
        {
            return container.CountEdgesAllGraph() * 2 / container.Size;
        }

        // Возвращает сумму минимальных путей. Не используется.
        public double MinPathsSum()
        {
            long[] pathsInfo = GetSubgraphsPathInfo(0, 0);
            return pathsInfo[0] + pathsInfo[2];
        }

        private List<List<int>> getMatches(List<int> _arr)
        {
            if (_arr.Count == 1)
                {
                    return new List<List<int>> { _arr };
                }
                var result = new List<List<int>>();
                foreach (var i in _arr)
                {
                    var ints = _arr.Select(b => b).ToList();
                    ints.Remove(i);
                    var matches = getMatches(ints);
                    foreach (var match in matches)
                    {
                        match.Add(i);
                        result.Add(match);
                    }
                }
                return result;
        }

        private List<List<int>> matchesFrom4 = null;
        private List<List<int>> matchesFrom5 = null;
        private IEnumerable<List<int>> getMatchesFrom4
        {
            get { return matchesFrom4 ?? (matchesFrom4 = getMatches(new List<int> { 0, 1, 2, 3 })); }
        }
        private IEnumerable<List<int>> getMatchesFrom5
        {
            get { return matchesFrom5 ?? (matchesFrom5 = getMatches(new List<int> { 0, 1, 2, 3, 4 })); }
        }

        public SortedDictionary<int, double> Count5Cycle(int nodeNumber, int level)
        {
            var arrayReturned = new SortedDictionary<int, double>();
            arrayReturned[0] = 0; // число циклов порядка 5
            arrayReturned[1] = 0; // число путей длиной 1 (ребер)
            arrayReturned[2] = 0; // число путей длиной 2
            arrayReturned[3] = 0; // число путей длиной 3

            if (level == container.Level)
            {
                return arrayReturned;
            }
            var array = new SortedDictionary<int, SortedDictionary<int, double>>();
            int bIndex = container.BranchIndex;

            for (int i = nodeNumber * bIndex; i < (nodeNumber + 1) * bIndex; ++i)
            {
                array[i] = Count5Cycle(i, level + 1);
                arrayReturned[0] += array[i][0];
                arrayReturned[1] += array[i][1];
                arrayReturned[2] += array[i][2];
                arrayReturned[3] += array[i][3];
            }

            BitArray node = container.TreeNode(level, nodeNumber);
            double powPK = Math.Pow(container.BranchIndex, container.Level - (level + 1));

            int indexFrom = nodeNumber * bIndex;
            int indexTo = (nodeNumber + 1) * bIndex;

            Func<List<int>, List<List<int>>> matchesFunc = null;
            matchesFunc = delegate(List<int> _arr)
            {
                if (_arr.Count == 1)
                {
                    return new List<List<int>> { _arr };
                }
                var result = new List<List<int>>();
                foreach (var i in _arr)
                {
                    var ints = _arr.Select(b => b).ToList();
                    ints.Remove(i);
                    var matches = matchesFunc(ints);
                    foreach (var match in matches)
                    {
                        match.Add(i);
                        result.Add(match);
                    }
                }
                return result;
            };

            Func<int, int, bool> isConnected =
                (i, j) => container.IsConnectedTwoBlocks(node, i - indexFrom, j - indexFrom);
            Func<int[], bool> isAllConnected = delegate(int[] indexes)
            {
                for (int i = 0; i < indexes.Length; i++)
                {
                    for (int j = i + 1; j < indexes.Length; j++)
                    {
                        if (!isConnected(i, j))
                        {
                            return false;
                        }
                    }
                }
                return true;
            };
            Func<List<int>, int[], bool> isCircle = delegate(List<int> vertexList, int[] indexes)
            {
                var count = vertexList.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!isConnected(indexes[vertexList[i]], indexes[vertexList[(i + 1) % count]]))
                    {
                        return false;
                    }
                }
                return true;
            };

            Func<double, double> fact = container.Factorial;
            SortedDictionary<int, double> ar = arrayReturned;
            SortedDictionary<int, SortedDictionary<int, double>> a = array;
            var n = (int)powPK;
            int n2 = n * n;
            int n3 = n2 * n;
            int n4 = n3 * n;
            int n5 = n4 * n;
            int n_2 = n * (n - 1) / 2;

            for (int i = indexFrom; i < indexTo; ++i)
            {
                for (int j = i + 1; j < indexTo; ++j)
                {
                    SortedDictionary<int, double> a_i = a[i];
                    SortedDictionary<int, double> a_j = a[j];
                    if (isConnected(i, j))
                    {
                        ar[0] += (a_i[2] * a_j[1]) / 2;
                        ar[0] += (a_i[1] * a_j[2]) / 2;
                        ar[0] += a_i[1] * n_2 * (n - 2);
                        ar[0] += a_j[1] * n_2 * (n - 2);
                        ar[0] += (a_i[3] / 2) * n;
                        ar[0] += (a_j[3] / 2) * n;

                        ar[1] += 2 * n2;
                        ar[2] += (a_i[1] + a_j[1]) * n * 2;
                        ar[2] += n_2 * n * 2;
                        ar[2] += n_2 * n * 2;
                        ar[3] += (a_i[2] + a_j[2]) * n * 2;
                        ar[3] += (a_i[1] + a_j[1]) * n_2 * 2;
                        ar[3] += (a_i[1] + a_j[1]) * (n - 2) * n * 2;
                        ar[3] += a_i[1] * a_j[1] * 2;
                        ar[3] += n_2 * n_2 * 8;
                    }
                    for (int k = j + 1; k < indexTo; k++)
                    {
                        SortedDictionary<int, double> a_k = a[k];
                        if (isAllConnected(new[] { i, j, k }))
                        {
                            var ds = new[] { a_i, a_j, a_k };
                            foreach (var d in ds)
                            {
                                ar[0] += d[2] * n2; // 9
                                ar[0] += d[1] * (n - 2) * n2; // 10
                                ar[0] += n_2 * n_2 * n * 4; // 12
                                ar[0] += d[1] * n_2 * n * 2; // 13

                                ar[3] += (d[1] / 2) * n2 * 12;
                            }
                            for (int l = 0; l < 3; l++)
                            {
                                var b1 = ds[(l + 1) % 3];
                                var b2 = ds[(l + 2) % 3];
                                ar[0] += b1[1] * b2[1] * n; // 11
                            }
                            ar[2] += n3 * fact(3);
                            ar[3] += n_2 * n2 * 12 * 3;
                        }
                        else // 13, 10
                        {
                            SortedDictionary<int, double> b1 = null, b2 = null, b = null;
                            if (isConnected(i, j) && isConnected(j, k) && !isConnected(i,k)) 
                            {
                                b1 = a_i;
                                b2 = a_k;
                                b = a_j;
                            }
                            else if (isConnected(i, j) && isConnected(i, k) && !isConnected(j,k))
                            {
                                b1 = a_j;
                                b2 = a_k;
                                b = a_i;
                            }
                            else if (isConnected(i, k) && isConnected(j, k) && !isConnected(i,j))
                            {
                                b1 = a_i;
                                b2 = a_j;
                                b = a_k;
                            }
                            if (b1 != null)
                            {
                                ar[0] += (b1[1] + b2[1]) * n_2 * n;
                                ar[0] += b[1] * (n - 2) * n2;

                                ar[2] += 2 * n3;
                                ar[3] += (b1[1] + b2[1]) * n2 * 2;
                                ar[3] += b[1] * n2 * 2;
                                ar[3] += n2 * n_2 * 8;
                            }
                        }
                        
                        for (int l = k + 1; l < indexTo; l++)
                        { 
                            var from4 = getMatchesFrom4;
                            var indexes = new[]{i, j, k, l};
                            foreach (List<int> ints in from4)
                            {
                                if (isCircle(ints, indexes))
                                {
                                    var b = a[indexes[ints[0]]];
                                    ar[0] += b[1]*n3;
                                    ar[0] += (n_2)*n3;
                                    ar[3] += n4;
                                }
                            }
                            for (int m = l+1; m < indexTo; m++)
                            {
                                var from5 = getMatchesFrom5;
                                var indexes1 = new[] { i, j, k, l, m };
                                double count = 0;
                                foreach (List<int> ints in from5)
                                {
                                    if (isCircle(ints, indexes1))
                                    {
                                        count += n5;
                                    }
                                }
                                ar[0] += count / 10;
                            }
                        }
                    }
                }
            }

            return arrayReturned;
        }
    }
}