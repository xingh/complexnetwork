﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using Core.Enumerations;
using Core.Attributes;

namespace Core.Result
{
    /// <summary>
    /// Represents the result of analyze for single Ensemble.
    /// </summary>
    public class EnsembleResult
    {
        public Dictionary<AnalyzeOption, object> Result { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static EnsembleResult AverageResults(List<RealizationResult> results)
        {
            EnsembleResult r = new EnsembleResult();
            r.Result = new Dictionary<AnalyzeOption, object>();

            int rCount = results.Count;

            foreach (AnalyzeOption option in results[0].Result.Keys)
            {
                AnalyzeOptionInfo info = ((AnalyzeOptionInfo[])option.GetType().GetField(option.ToString()).GetCustomAttributes(false))[0];
                Type t = info.RealizationResultType;

                if(t.Equals(typeof(Double)) || t.Equals(typeof(UInt32)))
                {
                    double temp = 0;
                    foreach (RealizationResult res in results)
                    {
                        temp += (double)(res.Result[option]) / rCount;
                    }
                    r.Result.Add(option, temp);
                }
                else if (t.Equals(typeof(BigInteger)))
                {
                    double temp = 0;
                    foreach (RealizationResult res in results)
                    {
                        //temp += (BigInteger.Parse(res.Result[option].ToString())) / rCount;
                    }
                    r.Result.Add(option, temp);
                }
                else if(t.Equals(typeof(List<Double>)))
                {
                    List<Double> temp = new List<double>(results[0].Result[option] as List<Double>);
                    for (int i = 0; i < temp.Count; ++i)
                        temp[i] /= rCount;

                    for (int i = 1; i < results.Count; ++i)
                    {
                        List<Double> l = results[i].Result[option] as List<Double>;
                        for (int j = 0; j < l.Count; ++j)
                            temp[j] += l[j] / rCount;
                    }
                    r.Result.Add(option, temp);
                }
                else if (t.Equals(typeof(SortedDictionary<Double, UInt32>)))
                {
                    SortedDictionary<Double, Double> temp = new SortedDictionary<double, double>();
                    foreach (RealizationResult res in results)
                    {
                        SortedDictionary<Double, UInt32> d = res.Result[option] as SortedDictionary<Double, UInt32>;
                        foreach (double k in d.Keys)
                        {
                            if (temp.ContainsKey(k))
                                temp[k] += (double)d[k] / rCount;
                            else
                                temp.Add(k, (double)d[k] / rCount);
                        }
                    }
                    r.Result.Add(option, temp);
                }
                else if (t.Equals(typeof(SortedDictionary<UInt32, UInt32>)))
                {
                    SortedDictionary<UInt32, Double> temp = new SortedDictionary<uint, double>();
                    foreach (RealizationResult res in results)
                    {
                        SortedDictionary<UInt32, UInt32> d = res.Result[option] as SortedDictionary<UInt32, UInt32>;
                        foreach (uint k in d.Keys)
                        {
                            if (temp.ContainsKey(k))
                                temp[k] += (double)d[k] / rCount;
                            else
                                temp.Add(k, (double)d[k] / rCount);
                        }
                    }
                    r.Result.Add(option, temp);
                }
                else if (t.Equals(typeof(SortedDictionary<UInt16, BigInteger>)))
                {
                    SortedDictionary<UInt16, Double> temp = new SortedDictionary<UInt16, Double>();
                    foreach (RealizationResult res in results)
                    {
                        SortedDictionary<UInt16, BigInteger> d = res.Result[option] as SortedDictionary<UInt16, BigInteger>;
                        foreach (UInt16 k in d.Keys)
                        {
                            if (temp.ContainsKey(k))
                                temp[k] += 0;
                            //temp[k] += (double)d[k] / rCount;
                            else
                                temp.Add(k, 0);
                                //temp.Add(k, (double)d[k] / rCount);
                        }
                    }
                    r.Result.Add(option, temp);
                }
            }

            return r;
        }
    }
}
