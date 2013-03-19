﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomGraph.Common.Model.Generation;
using RandomGraph.Common.Model;
using RandomGraph.Common.Model.Result;

namespace CommonLibrary.Model.Result
{
    public class ResultAssembly
    {
        public ResultAssembly()
        {
            ID = Guid.NewGuid();
            FileName = "none";
            Results = new List<AnalizeResult>();
            GenerationParams = new Dictionary<GenerationParam, object>();
            AnalyzeOptionParams = new Dictionary<AnalyzeOptionParam, object>();
        }

        public Guid ID { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public string FileName { get; set; }
        public List<AnalizeResult> Results { get; set; }
        public Type ModelType { get; set; }
        public string ModelName { get; set; }
        public Dictionary<GenerationParam, object> GenerationParams { get; set; }
        public AnalyseOptions AnalizeOptions { get; set; }
        public Dictionary<AnalyzeOptionParam, object> AnalyzeOptionParams;
    }
}
