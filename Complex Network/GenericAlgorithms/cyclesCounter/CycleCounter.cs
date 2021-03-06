﻿/**
 * @File CycleCounter.cs
 * 
 * @Author Hovhannes Antonyan
 */

using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using System.IO;
using System.Text;

namespace Algorithms
{
    public interface ICycleCounter
    {
        long getCycleCount(int cycleLength);
        IDictionary<int, long> getCycleCount(int startRange, int endRange);
    }

    /**
     * The class is responsible for calculation of the cycles count with 
     * specified length in a graph.
     */
    public class CycleCounter : ICycleCounter
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(CycleCounter));

        // The actual class which performs the calculation
        private ICycleCounterInternal _counter;

        public CycleCounter(string fileName)
        {
            List<List<bool>> matrix = Container.get_data(fileName);
            INeighbourshipContainer container = new Container(matrix);
            _counter = new CycleCounterParallel(container);
            //_counter = new CycleCounterSingleThreaded(container);
        }

        public CycleCounter(List<List<bool>> matrix)
        {
            INeighbourshipContainer container = new Container(matrix);
            _counter = new CycleCounterParallel(container);
            //_counter = new CycleCounterSingleThreaded(container);
        }

        public IDictionary<int, long> getCycleCount(int startRange, int endRange)
        {
            if (startRange > endRange)
            {
                throw new Exception("Cannot calculate cycle count. Start range cannot be greater than endRange.");
            }
            IDictionary<int/*length*/, long/*count*/> counts = new SortedDictionary<int, long>();
            int i = startRange;
            try
            {
                for (; i <= endRange; ++i)
                {
                    long count = getCycleCount(i);
                    counts.Add(i, count);
                }
            }
            catch (Exception e)
            {
                StringBuilder builder = new StringBuilder("Found following cycles:\n");
                foreach (KeyValuePair<int, long> entry in counts) {
                    builder.Append("\tlength=" + entry.Key + "->count=" + entry.Value + "\n");
                }
                if (i > startRange)
                {
                    logger.Info(builder.ToString());
                }
                throw e;
            }
            return counts;
        }

        /**
         * Returns cycles count in the graph which length is equal 
         * to the specified 'cycleLength' argument
         * @pre cycleLength >= 3
         */
        public long getCycleCount(int cycleLength)
        {
            long count = 0;
            try
            {
                count = _counter.getCycleCount(cycleLength);
            }
            catch (ThreadInterruptedException e)
            {
                logger.Error("The thread was interrupted during calculation of cycles " + cycleLength);
                throw e;
            }
            catch (NotSupportedException e)
            {
                logger.Error("Cannot calculate cycle count. The host does not fully support thread pools."
                    + "Use single threaded version of the class");
                throw e;
            }
            catch (Exception e)
            {
                logger.Error("Exception occurred during calculation of cycles " + cycleLength);
                throw e;
            }
            return count;
        }
    }

    interface ICycleCounterInternal
    {
        long getCycleCount(int cycleLength);
    }

    interface INeighbourshipContainer
    {
        // Get functions //
        int Size
        {
            get;
        }

        IDictionary<int, List<int>> Neighbourship
        {
            get;
        }
    }

    // Inner class which holds the graph
    class Container : INeighbourshipContainer
    {
        private int _size; // number of vertices
        private IDictionary<int, List<int>> _neighbourship; // list of neighbours for each vertex  
        private List<List<bool>> _matrix;

        public Container(List<List<bool>> matrix)
        {
            validateMatrix(matrix);
            _matrix = matrix;
            _size = matrix.Count;
            _neighbourship = new SortedDictionary<int, List<int>>();
            List<bool> neighbourshipOfIVertex = new List<bool>();
            for (int i = 0; i < matrix.Count; i++)
            {
                neighbourshipOfIVertex = matrix[i];
                setDataToDictionary(i, neighbourshipOfIVertex);
            }
        }

        private void validateMatrix(List<List<bool>> matrix)
        {
            for (int i = 0; i < matrix.Count; ++i)
            {
                if (matrix[i].Count != matrix.Count)
                {
                    throw new System.Exception("Given matrix is not well formed");
                }
                for (int j = 0; j < matrix.Count; ++j)
                {
                    if (i != j && matrix[i][j] != matrix[j][i])
                    {
                        throw new System.Exception("Given matrix is not well formed");
                    }
                }
            }
        }

        public int Size
        {
            get { return _size; }
        }

        public IDictionary<int, List<int>> Neighbourship
        {
            get { return _neighbourship; }
        }

        public bool areConnected(int vertex1, int vertex2)
        {
            return _matrix[vertex1][vertex2];
        }

        public static List<List<bool>> get_data(string filename)
        {
            List<List<bool>> matrix = new List<List<bool>>();
            using (StreamReader streamreader = new StreamReader(filename))
            {
                string contents;
                while ((contents = streamreader.ReadLine()) != null)
                {
                    string[] split = System.Text.RegularExpressions.Regex.Split(contents,
                            "\\s+", System.Text.RegularExpressions.RegexOptions.None);
                    List<bool> tmp = new List<bool>();
                    for (int i = 0; i < split.Length - 1; ++i)
                    {
                        string s = split[i];
                        if (s.Equals("0"))
                        {
                            tmp.Add(false);
                        }
                        else
                        {
                            tmp.Add(true);
                        }
                    }
                    if (!split[split.Length - 1].Equals(""))
                    {
                        string s = split[split.Length - 1];
                        if (s.Equals("0"))
                        {
                            tmp.Add(false);
                        }
                        else
                        {
                            tmp.Add(true);
                        }
                    }
                    matrix.Add(tmp);
                }
            }
            return matrix;
        }

        private void setDataToDictionary(int index, List<bool> neighbourshipOfIVertex)
        {
            _neighbourship[index] = new List<int>();
            for (int j = 0; j < _size; j++)
            {
                if ((bool)neighbourshipOfIVertex[j] == true && index != j)
                {
                    _neighbourship[index].Add(j);
                }
            }
        }
    }
}
