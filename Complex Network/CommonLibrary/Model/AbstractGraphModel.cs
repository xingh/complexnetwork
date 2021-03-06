﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Serialization;
using System.Numerics;

using RandomGraph.Common.Model.Generation;
using RandomGraph.Common.Model.Result;
using RandomGraph.Common.Model.Status;
using RandomGraph.Core.Events;
using CommonLibrary.Model.Events;
using CommonLibrary.Model.Util;
using CommonLibrary.Model;
using AnalyzerFramework.Manager.ModelRepo;

namespace RandomGraph.Common.Model
{
    [KnownType("GetKnownModelTypes")]
    // Базовый абстрактный класс для имплементации модели графа.
    public abstract class AbstractGraphModel
    {
        // Генератор графа.
        protected AbstractGraphGenerator generator;
        // Анализатор графа.
        protected AbstarctGraphAnalyzer analyzer;
        // События.
        public event GraphProgressEventHandler Progress;
        public event GraphGeneratedDelegate GraphGenerated;

        public AbstractGraphModel() { }

        // Конструктор, в который входные параметры переходят от конструкторов дочерных классов.
        // Передаются параметры генерации (подразумевается динамическая генерация).
        public AbstractGraphModel(Dictionary<GenerationParam, object> genParam, 
            AnalyseOptions options, 
            Dictionary<AnalyzeOptionParam, Object> analyzeOptionsValues)
        {
            GenerationParamValues = genParam;
            AnalyzeOptions = options;
            AnalyzeOptionsValues = analyzeOptionsValues;

            CurrentStatus = new GraphProgressStatus();
            CurrentStatus.GraphProgress = GraphProgress.Initializing;
        }

        public void SetID(int ID)
        {
            this.ID = ID;
            Result = new AnalizeResult()
            {
                InstanceID = ID
            };
        }        

        // Свойства.

        // Директория трассировки.
        public string TracingPath { get; set; }

        // Имя модели.
        public string ModelName { get; set; }
        // Уникальных идентификатор данной модели.
        public int ID { get; set; }
        // Текущий статус процесса выполнения.
        public GraphProgressStatus CurrentStatus { get; set; }        
        // Ссылка на обьект графа, созданный в процессе генерации.
        public object Graph { get; set; }
        // Список параметров генерации для данной модели.
        public List<GenerationParam> RequiredGenerationParams { get; set; }
        // Свойства, которые доступны для вычисления для данной модели.
        public AnalyseOptions AvailableOptions { get; set; }
        // Значения параметров генерации.
        public Dictionary<GenerationParam, object> GenerationParamValues { get; set; }
        // Свойства, которые должны вычисляться.
        public AnalyseOptions AnalyzeOptions { get; set; }
        // Значения свойств.
        public Dictionary<AnalyzeOptionParam, Object> AnalyzeOptionsValues { get; set; }
        // Результат вычислений.
        public AnalizeResult Result { get; set; }

        //Permanet Status
        virtual public bool PermanentStatus { get; set; }

        // Защищенная часть.

        // Функция вызывается в методе StartGenerate в отдельном потоке.
        // Для получения параметров генерации (динамическая генерация) используется GenerationParamValues dictionary.
        // Для получения матрицы смежности (статическая генерация) используется NeighbourshipMatrix ArrayList.
        protected void GenerateModel()
        {
            InvokeProgressEvent(GraphProgress.StartingGeneration, 0);

            try
            {
                if (GenerationParamValues != null)
                {
                    if (GenerationParamValues.ContainsKey(GenerationParam.Permanent)
                        && (bool)GenerationParamValues[GenerationParam.Permanent])
                    {
                        // Динамическая генерация с параметром Permanent
                        generator.PermanentGeneration(GenerationParamValues);
                    }
                    else
                    {
                        // Динамическая или статическая генерация
                        generator.Generation(GenerationParamValues);
                    }
                }
                else
                {
                    throw new SystemException("Generation type is not correct!");
                }

                InvokeProgressEvent(GraphProgress.GenerationDone, 10);
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                InvokeFailureProgressEvent(GraphProgress.GenerationFailed, "ENTER FAIL REASON HERE");
                //RETHROW EXCEPTION 
            }
            finally
            {
                //Place clean up code here
            }
        }

        // Функция вызывается в методе StartAnalize в отдельном потоке. 
        // Для получения выбранных опций анализа используется флаг перечисления AnalyzeOptions.
        // После окончания анализа обьект Result должен иметь значение.
        protected void AnalyzeModel()
        {
            InvokeProgressEvent(GraphProgress.StartingAnalyzing);

            analyzer.Container = generator.Container;
            Result.Size = generator.Container.Size;

            try
            {
                if ((AnalyzeOptions & AnalyseOptions.AveragePath) == AnalyseOptions.AveragePath)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 15, "Average Path");
                    Result.Result[AnalyseOptions.AveragePath] = analyzer.GetAveragePath();
                }

                if ((AnalyzeOptions & AnalyseOptions.Diameter) == AnalyseOptions.Diameter)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 20, "Diameter");
                    Result.Result[AnalyseOptions.Diameter] = analyzer.GetDiameter();
                }
 
                if ((AnalyzeOptions & AnalyseOptions.Cycles3) == AnalyseOptions.Cycles3)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 25, "Cycles of order 3");
                    Result.Result[AnalyseOptions.Cycles3] = analyzer.GetCycles3();
                }

                if ((AnalyzeOptions & AnalyseOptions.Cycles4) == AnalyseOptions.Cycles4)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 30, "Cycles of order 4");
                    Result.Result[AnalyseOptions.Cycles4] = analyzer.GetCycles4();
                }

                if ((AnalyzeOptions & AnalyseOptions.Cycles5) == AnalyseOptions.Cycles5)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 33, "Cycles of order 5");
                    Result.Result[AnalyseOptions.Cycles5] = analyzer.GetCycles5();
                }

                if ((AnalyzeOptions & AnalyseOptions.CycleEigen3) == AnalyseOptions.CycleEigen3)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 35, "Cycles of order 3 (Eigen)");
                    Result.Result[AnalyseOptions.CycleEigen3] = analyzer.GetCyclesEigen3();
                }

                if ((AnalyzeOptions & AnalyseOptions.CycleEigen4) == AnalyseOptions.CycleEigen4)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 40, "Cycles of order 4 (Eigen)");
                    Result.Result[AnalyseOptions.CycleEigen4] = analyzer.GetCyclesEigen4();
                }

                if ((AnalyzeOptions & AnalyseOptions.EigenValue) == AnalyseOptions.EigenValue)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 45, "Calculating EigenValues");
                    Result.EigenVector = analyzer.GetEigenValues();
                }

                if ((AnalyzeOptions & AnalyseOptions.DistEigenPath) == AnalyseOptions.DistEigenPath)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 50, "Distances between Eigen Values");
                    Result.DistancesBetweenEigenValues = analyzer.GetDistEigenPath();
                }

                if ((AnalyzeOptions & AnalyseOptions.DegreeDistribution) == AnalyseOptions.DegreeDistribution)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 55, "Degree Distribution");
                    Result.VertexDegree = analyzer.GetDegreeDistribution();
                }

                if ((AnalyzeOptions & AnalyseOptions.ClusteringCoefficient) == AnalyseOptions.ClusteringCoefficient)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 60, "Clustering Coefficient");
                    Result.Coefficient = analyzer.GetClusteringCoefficient();
                }

                if ((AnalyzeOptions & AnalyseOptions.ConnSubGraph) == AnalyseOptions.ConnSubGraph)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 65, "Connected Subgraphs Orders");
                    Result.Subgraphs = analyzer.GetConnSubGraph();
                }

                if ((AnalyzeOptions & AnalyseOptions.FullSubGraph) == AnalyseOptions.FullSubGraph)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 65, "Full Subgraphs Orders");
                    Result.FullSubgraphs = analyzer.GetFullSubGraph();
                }

                if ((AnalyzeOptions & AnalyseOptions.MinPathDist) == AnalyseOptions.MinPathDist)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 70, "Minimal Path Distance Distribution");
                    Result.DistanceBetweenVertices = analyzer.GetMinPathDist();
                }

                if ((AnalyzeOptions & AnalyseOptions.Cycles) == AnalyseOptions.Cycles)
                {
                    int maxValue = (Int16)AnalyzeOptionsValues[AnalyzeOptionParam.CyclesHigh];
                    int minvalue = (Int16)AnalyzeOptionsValues[AnalyzeOptionParam.CyclesLow];

                    InvokeProgressEvent(GraphProgress.Analyzing, 75, "Cycles of " + minvalue + "-" + maxValue + "degree");
                    Result.Cycles = analyzer.GetCycles(minvalue, maxValue);
                }

                if ((AnalyzeOptions & AnalyseOptions.TriangleCountByVertex) == AnalyseOptions.TriangleCountByVertex)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 80, "Triangles Distribution");
                    Result.TriangleCount = analyzer.GetTrianglesDistribution();
                }

                if ((AnalyzeOptions & AnalyseOptions.TriangleTrajectory) == AnalyseOptions.TriangleTrajectory)
                {
                    InvokeProgressEvent(GraphProgress.Analyzing, 85, "Triangle Trajectory");
                    try
                    {
                        double constant = (Double)AnalyzeOptionsValues[AnalyzeOptionParam.TrajectoryMu];
                        BigInteger stepCount = BigInteger.Parse(AnalyzeOptionsValues[AnalyzeOptionParam.TrajectoryStepCount].ToString());
                        bool keepDistribution = true; 
                        if (AnalyzeOptionsValues.ContainsKey(AnalyzeOptionParam.KeepDegreedistribution))
                        {
                            keepDistribution = (bool)AnalyzeOptionsValues[AnalyzeOptionParam.KeepDegreedistribution];
                        }
                        Result.TriangleTrajectory = analyzer.GetTrianglesTrajectory(constant, stepCount, keepDistribution);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Cannot Parse parameter");
                    }
                }

                if ((AnalyzeOptions & AnalyseOptions.Motifs) == AnalyseOptions.Motifs)
                {
                    int maxValue = (Int16)AnalyzeOptionsValues[AnalyzeOptionParam.MotifsHigh];
                    int minvalue = (Int16)AnalyzeOptionsValues[AnalyzeOptionParam.MotifsLow];
                    InvokeProgressEvent(GraphProgress.Analyzing, 90, "Motiv of " + minvalue + "-" + maxValue + "degree");
                    Result.MotivesCount = analyzer.GetMotivs(minvalue, maxValue);
                }

                InvokeProgressEvent(GraphProgress.AnalyzingDone, 90);

            }
            catch (Exception ex)
            {
                InvokeFailureProgressEvent(GraphProgress.AnalyzingFailed, ex.Message);
                //RETHROW EXCEPTION
            }
            finally
            {
                InvokeProgressEvent(GraphProgress.Done, 100);
            }
        }

        // Получение матрицы смежности из контейнера.
        public bool[,] GetMatrix()
        {
            return analyzer.Container.GetMatrix();
        }

        public int[][] GetBranches()
        {
            return analyzer.Container.GetBranches();
        }

        /// <summary>
        /// Check input generation parameters. 
        /// </summary>
        public abstract bool CheckGenerationParams(int instances);        

        /// <summary>
        /// Prints details information of parameters into the form.
        /// </summary>
        public abstract string GetParamsInfo();

        /// <summary>
        /// Create and return object of specific type
        /// </summary>
        public abstract AbstractGraphModel Clone();

        /// <summary>
        /// Dump generated graph matrix into file
        /// </summary>
        public void StartTrace(int instanceIndex, string modelName, string jobName)
        {
            string dir = TracingPath + "\\" + modelName + "\\";
            string filePath = dir + jobName + "_" + instanceIndex + "_dump.txt";
            System.IO.Directory.CreateDirectory(dir);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
            {
                bool[,] matrix = GetMatrix();
                for (int i = 0; i < matrix.GetLength(0); ++i)
                {
                    for (int j = 0; j < matrix.GetLength(1); ++j)
                    {
                        if (matrix[i, j])
                        {
                            file.Write(1 + " ");
                        }
                        else
                        {
                            file.Write(0 + " ");
                        }
                    }
                    file.WriteLine("");
                }
            }

            try
            {
                int[][] branches = GetBranches();

                filePath = dir + jobName + "_" + instanceIndex + "_dump_branches.txt";
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
                {
                    for (int i = 0; i < branches.Length; i++)
                    {
                        for (int k = 0; k < branches[i].Length; k++)
                        {
                            writer.Write(branches[i][k]);
                            writer.Write(" ");
                        }
                        writer.WriteLine();
                    }
                }
            }
            catch (NotImplementedException) { }
        }

        /// <summary>
        /// ???.
        /// </summary>
        protected void OnModelProgress(GraphProgressEventArgs args)
        {
            if (Progress != null)
            {
                CurrentStatus = args.Progress;
                args.Progress.ID = ID;
                Progress(this, args);
            }
        }

        /// <summary>
        /// This method is invoked if generation are completed
        /// </summary>
        protected void OnGraphGenerated(GraphGeneratedArgs e)
        {
            if (GraphGenerated != null)
            {
                GraphGenerated(this, e);
            }
        }

        #region Helper Methods

        protected void InvokeProgressEvent(GraphProgress progress)
        {
            InvokeProgressEvent(progress, null, null);
        }

        protected void InvokeProgressEvent(GraphProgress progress, String targetName)
        {
            InvokeProgressEvent(progress, null, targetName);
        }

        protected void InvokeProgressEvent(GraphProgress progress, int? percent)
        {
            InvokeProgressEvent(progress, percent, null);
        }
        protected void InvokeProgressEvent(GraphProgress progress, int? percent, String targetName)
        {
            CurrentStatus.GraphProgress = progress;
            CurrentStatus.Percent = percent != null ? percent : CurrentStatus.Percent;
            CurrentStatus.TargetName = targetName;
            CurrentStatus.FailReason = null;
            OnModelProgress(new GraphProgressEventArgs(CurrentStatus));
        }

        public void InvokeFailureProgressEvent(GraphProgress progress, String failReason)
        {
            if (CurrentStatus == null)
            {
                CurrentStatus = new GraphProgressStatus();
            }
            CurrentStatus.GraphProgress = progress;
            CurrentStatus.FailReason = failReason;
            OnModelProgress(new GraphProgressEventArgs(CurrentStatus));
        }

        #endregion

        #region IGraphModel Members

        /// Triggers analyze start process,
        /// and can be called only in case when 
        /// generation is fully completed
        public void StartAnalize()
        {
            if (CurrentStatus.GraphProgress != GraphProgress.GenerationDone)
            {
                return;
            }
            AnalyzeModel();
        }

        /// Начинается генерация графа.
        public void StartGenerate()
        {
            if (CurrentStatus.GraphProgress != GraphProgress.Ready)
            {
                return;
            }
            GenerateModel();
        }

        public void UpdateGeneratedMatrix()
        {
            bool[,] matrix = GetMatrix();
            OnGraphGenerated(new GraphGeneratedArgs(new GraphTable(matrix)));
        }

        public virtual void Dispose()
        {

        }

        #endregion

        /// <summary>
        /// This method finds .dll files of all Models and imported to the system.
        /// </summary>
        private static Type[] GetKnownModelTypes()
        {
            return ModelRepository.GetInstance().GetAvailableModelTypes().ToArray();
        }
    }
}
