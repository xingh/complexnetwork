﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using Core.Enumerations;

namespace Core.Model
{
    /// <summary>
    /// Abstract class presenting random network analyzer.
    /// </summary>
    public abstract class AbstractNetworkAnalyzer : INetworkAnalyzer
    {
        protected AbstractNetwork network;

        public abstract INetworkContainer Container { get; set; }

        public AbstractNetworkAnalyzer(AbstractNetwork n)
        {
            network = n;
        }

        public UInt32 CalculateEdgesCount()
        {
            return CalculateEdgesCountOfNetwork();
        }

        public Object CalculateOption(AnalyzeOption option)
        {
            switch (option)
            {
                case AnalyzeOption.AvgClusteringCoefficient:
                    return CalculateAverageClusteringCoefficient();
                case AnalyzeOption.AvgDegree:
                    return CalculateAverageDegree();
                case AnalyzeOption.AvgPathLength:
                    return CalculateAveragePath();
                case AnalyzeOption.ClusteringCoefficientDistribution:
                    return CalculateClusteringCoefficientDistribution();
                case AnalyzeOption.CompleteComponentDistribution:
                    return CalculateCompleteComponentDistribution();
                case AnalyzeOption.ConnectedComponentDistribution:
                    return CalculateConnectedComponentDistribution();
                case AnalyzeOption.CycleDistribution:
                    // TODO
                    return CalculateCycleDistribution(1, 1);
                case AnalyzeOption.Cycles3:
                    return CalculateCycles3();
                case AnalyzeOption.Cycles3Eigen:
                    return CalculateCycles3Eigen();
                case AnalyzeOption.Cycles3Trajectory:
                    return CalculateCycles3Trajectory();
                case AnalyzeOption.Cycles4:
                    return CalculateCycles4();
                case AnalyzeOption.Cycles4Eigen:
                    return CalculateCycles4Eigen();
                case AnalyzeOption.DegreeDistribution:
                    return CalculateDegreeDistribution();
                case AnalyzeOption.Diameter:
                    return CalculateDiameter();
                case AnalyzeOption.DistanceDistribution:
                    return CalculateDistanceDistribution();
                case AnalyzeOption.EigenDistanceDistribution:
                    return CalculateEigenDistanceDistribution();
                case AnalyzeOption.EigenValues:
                    return CalculateEigenValues();
                case AnalyzeOption.TriangleByVertexDistribution:
                    return CalculateTriangleByVertexDistribution();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Calculates count of edges of the network.
        /// </summary>
        /// <returns>Count of edges.</returns>
        protected virtual UInt32 CalculateEdgesCountOfNetwork()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the average path length of the network.
        /// </summary>
        /// <returns>Average path length.</returns>
        protected virtual Double CalculateAveragePath()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the diameter of the network.
        /// </summary>
        /// <returns>Diameter.</returns>
        protected virtual UInt32 CalculateDiameter()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the average value of vertex degrees in the network.
        /// </summary>
        /// <returns>Average degree.</returns>
        protected virtual Double CalculateAverageDegree()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the average value of vertex clustering coefficients in the network.  
        /// </summary>
        /// <returns>Average clustering coefficient.</returns>
        protected virtual Double CalculateAverageClusteringCoefficient()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the number of cycles of length 3 in the network.
        /// </summary>
        /// <returns>Number of cycles 3.</returns>
        protected virtual Double CalculateCycles3()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the number of cycles of length 4 in the network.
        /// </summary>
        /// <returns>Number of cycles 4.</returns>
        protected virtual Double CalculateCycles4()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the eigenvalues of adjacency matrix of the network.
        /// </summary>
        /// <returns>List of eigenvalues.</returns>
        protected virtual List<Double> CalculateEigenValues()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the number of cycles of length 3 in the network using eigenvalues.
        /// </summary>
        /// <returns>Number of cycles 3.</returns>
        protected virtual Double CalculateCycles3Eigen()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the number of cycles of length 4 in the network using eigenvalues.
        /// </summary>
        /// <returns>Number of cycles 4.</returns>
        protected virtual Double CalculateCycles4Eigen()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates distances between eigenvalues.
        /// </summary>
        /// <returns>(distance, count) pairs.</returns>
        protected virtual SortedDictionary<Double, UInt32> CalculateEigenDistanceDistribution()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates degrees of vertices in the network.
        /// </summary>
        /// <returns>(degree, count) pairs.</returns>
        protected virtual SortedDictionary<UInt32, UInt32> CalculateDegreeDistribution()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates clustering coefficients of vertices in the network.
        /// </summary>
        /// <returns>(clustering coefficient, count) pairs.</returns>
        protected virtual SortedDictionary<Double, UInt32> CalculateClusteringCoefficientDistribution()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates counts of connected components in the network.
        /// </summary>
        /// <returns>(order of connected component, count) pairs.</returns>
        protected virtual SortedDictionary<UInt32, UInt32> CalculateConnectedComponentDistribution()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates counts of complete components in the network.
        /// </summary>
        /// <returns>(order of complete component, count) pairs.</returns>
        protected virtual SortedDictionary<UInt32, UInt32> CalculateCompleteComponentDistribution()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates minimal path lengths in the network.
        /// </summary>
        /// <returns>(minimal path length, count) pairs.</returns>
        protected virtual SortedDictionary<UInt32, UInt32> CalculateDistanceDistribution()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates counts of triangles by vertices in the network.
        /// </summary>
        /// <returns>(triangle count, count) pairs.</returns>
        protected virtual SortedDictionary<UInt32, UInt32> CalculateTriangleByVertexDistribution()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the counts of cycles in the network.
        /// </summary>
        /// <param name="lowBound">Minimal length of cycle.</param>
        /// <param name="hightBound">Maximal length of cycle.</param>
        /// <returns>(cycle length, count) pairs.</returns>
        protected virtual SortedDictionary<UInt16, Double> CalculateCycleDistribution(UInt16 lowBound, UInt16 hightBound)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the counts of cycles 3 in the network during evolution process.
        /// </summary>
        /// <param name="stepCount">Count of steps of evolution.</param>
        /// <param name="nu">Managment parameter.</param>
        /// <param name="permanentDistribution">Indicates if degree distribution must be permanent during evolution process.</param>
        /// <returns>(step, cycles 3 count)</returns>
        protected virtual SortedDictionary<UInt32, Double> CalculateCycles3Trajectory()
        {
            throw new NotImplementedException();
        }
    }
}
