﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using Core.Enumerations;

namespace Core.Result
{
    /// <summary>
    /// Represents the result of analyze for single Ensemble.
    /// </summary>
    public class EnsembleResult
    {
        public Dictionary<AnalyzeOption, object> Result { get; set; }

        /*public Double AvgPathLength { get; set; }
        public UInt32 Diameter { get; set; }
        public Double AvgDegree { get; set; }
        public Double AvgClusteringCoefficient { get; set; }
        public BigInteger Cycles3 { get; set; }
        public BigInteger Cycles4 { get; set; }
        public List<Double> EigenValues { get; set; }
        public BigInteger Cycles3Eigen { get; set; }
        public BigInteger Cycles4Eigen { get; set; }
        public SortedDictionary<Double, UInt32> EigenDistanceDistribution { get; set; }
        public SortedDictionary<UInt32, UInt32> DegreeDistribution { get; set; }
        public SortedDictionary<Double, UInt32> ClusteringCoefficientDistribution { get; set; }
        public SortedDictionary<UInt32, UInt32> ConnectedComponentDistribution { get; set; }
        public SortedDictionary<UInt32, UInt32> CompleteComponentDistribution { get; set; }
        public SortedDictionary<UInt32, UInt32> DistanceDistribution { get; set; }
        public SortedDictionary<UInt32, UInt32> TriangleByVertexDistribution { get; set; }
        public SortedDictionary<UInt16, BigInteger> CycleDistribution { get; set; }*/
    }
}