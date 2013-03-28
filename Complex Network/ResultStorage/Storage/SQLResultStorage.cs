﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Data.Common;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using RandomGraph.Common.Storage;
using CommonLibrary.Model.Result;
using RandomGraph.Common.Model.Result;
using RandomGraph.Common.Model;
using CommonLibrary.Model.Attributes;
using RandomGraph.Common.Model.Generation;
using Model.HierarchicModel;
using System.Collections;
using System.Globalization;
using log4net;

namespace ResultStorage.Storage
{
    // Реализация хранилища данных - SQL DB.
    public class SQLResultStorage : ResultStorage
    {
        // Организация работы с лог файлом.
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(SQLResultStorage));

        // Информация для соединения с SQL сервером.
        private ConnectionStringSettings connSettings;
        private DbProviderFactory provider;

        // Конструктор, который получает строку соединения.
        public SQLResultStorage(ConnectionStringSettings connectionStringSettings)
        {
            string dp = connectionStringSettings.ProviderName;
            this.connSettings = connectionStringSettings;
            provider = DbProviderFactories.GetFactory(dp);
        }

        private string GetConnectionString()
        {
            return connSettings.ConnectionString;
        }

        #region IResultStorage Members

        // Сохранение сборки в БД.
        public override void Save(ResultAssembly assembly)
        {
            log.Info("Saving analyze results int DB.");

            log.Info("Openning connection.");
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                try
                {
                    conn.Open();
                }
                catch (Exception)
                {
                    log.Info("Could not open a connection with DB.");
                    return;
                }

                log.Info("Saving data to Assemblies table.");
                using (DbCommand cmd = conn.CreateCommand())
                {
                    string sqlQuery = "INSERT INTO Assemblies(AssemblyID,ModelID,Name,Date,NetworkSize,FileName)" +
                        "VALUES(@AssemblyID,@ModelID,@Name,getDate(),@NetworkSize,@FileName)";
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    DbParameter dpID = provider.CreateParameter();
                    dpID.ParameterName = "AssemblyID";
                    dpID.Value = assembly.ID;
                    cmd.Parameters.Add(dpID);

                    DbParameter dpModelID = provider.CreateParameter();
                    dpModelID.ParameterName = "ModelID";
                    dpModelID.Value = GetModelID(assembly.ModelType);
                    cmd.Parameters.Add(dpModelID);

                    DbParameter dpName = provider.CreateParameter();
                    dpName.ParameterName = "Name";
                    dpName.Value = assembly.Name;
                    cmd.Parameters.Add(dpName);

                    DbParameter dpSize = provider.CreateParameter();
                    dpSize.ParameterName = "NetworkSize";
                    dpSize.Value = assembly.Size;
                    cmd.Parameters.Add(dpSize);

                    DbParameter dpFileName = provider.CreateParameter();
                    dpFileName.ParameterName = "FileName";
                    dpFileName.Value = assembly.FileName;
                    cmd.Parameters.Add(dpFileName);

                    cmd.ExecuteNonQuery();
                }

                log.Info("Saving generation params values in DB.");
                foreach (GenerationParam genParameter in assembly.GenerationParams.Keys)
                {
                    using (DbCommand cmd = conn.CreateCommand())
                    {
                        string sqlQuery = "INSERT INTO GenerationParamValues(AssemblyID,GenerationParamID,Value) " +
                                            "VALUES(@AssemblyID,@GenerationParamID,@Value)";
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        DbParameter dpAssemblyID = provider.CreateParameter();
                        dpAssemblyID.ParameterName = "AssemblyID";
                        dpAssemblyID.Value = assembly.ID;
                        cmd.Parameters.Add(dpAssemblyID);

                        DbParameter dpGenerationParamID = provider.CreateParameter();
                        dpGenerationParamID.ParameterName = "GenerationParamID";
                        dpGenerationParamID.Value = Convert.ToInt32(genParameter);
                        cmd.Parameters.Add(dpGenerationParamID);

                        DbParameter dpValue = provider.CreateParameter();
                        dpValue.ParameterName = "Value";
                        dpValue.Value = assembly.GenerationParams[genParameter].ToString();
                        cmd.Parameters.Add(dpValue);

                        cmd.ExecuteNonQuery();
                    }
                }

                // !исправить!
                log.Info("Saving analyze option params values in DB.");
                using (DbCommand cmd = conn.CreateCommand())
                {
                    string sqlQuery = "INSERT INTO AnalyzeOptionParamValues(AssemblyID,AnalyzeOptionParamID,Value) " +
                                        "VALUES(@AssemblyID,@AnalyzeOptionParamID,@Value)";
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    DbParameter dpAssemblyID = provider.CreateParameter();
                    dpAssemblyID.ParameterName = "AssemblyID";
                    dpAssemblyID.Value = assembly.ID;
                    cmd.Parameters.Add(dpAssemblyID);

                    DbParameter dpGenerationParamID = provider.CreateParameter();
                    dpGenerationParamID.ParameterName = "AnalyzeOptionParamID";
                    dpGenerationParamID.Value = 5;
                    cmd.Parameters.Add(dpGenerationParamID);

                    DbParameter dpValue = provider.CreateParameter();
                    dpValue.ParameterName = "Value";
                    dpValue.Value = assembly.AnalyzeOptionParams[AnalyzeOptionParam.TrajectoryMu].ToString();
                    cmd.Parameters.Add(dpValue);

                    cmd.ExecuteNonQuery();
                }

                using (DbCommand cmd = conn.CreateCommand())
                {
                    string sqlQuery = "INSERT INTO AnalyzeOptionParamValues(AssemblyID,AnalyzeOptionParamID,Value) " +
                                        "VALUES(@AssemblyID,@AnalyzeOptionParamID,@Value)";
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    DbParameter dpAssemblyID = provider.CreateParameter();
                    dpAssemblyID.ParameterName = "AssemblyID";
                    dpAssemblyID.Value = assembly.ID;
                    cmd.Parameters.Add(dpAssemblyID);

                    DbParameter dpGenerationParamID = provider.CreateParameter();
                    dpGenerationParamID.ParameterName = "AnalyzeOptionParamID";
                    dpGenerationParamID.Value = 6;
                    cmd.Parameters.Add(dpGenerationParamID);

                    DbParameter dpValue = provider.CreateParameter();
                    dpValue.ParameterName = "Value";
                    dpValue.Value = assembly.AnalyzeOptionParams[AnalyzeOptionParam.TrajectoryStepCount].ToString();
                    cmd.Parameters.Add(dpValue);

                    cmd.ExecuteNonQuery();
                }

                log.Info("Saving data to AssemblyResults table.");
                foreach (AnalizeResult result in assembly.Results)
                {
                    int resultsID = 0;
                    using (DbCommand cmd = conn.CreateCommand())
                    {
                        string sqlQuery = "INSERT INTO AssemblyResults(AssemblyID) VALUES(@AssemblyID) " +
                                            "SELECT ResultsID FROM AssemblyResults WHERE ResultsID=SCOPE_IDENTITY()";
                        cmd.CommandText = sqlQuery;
                        cmd.CommandType = CommandType.Text;

                        DbParameter dpAssemblyID = provider.CreateParameter();
                        dpAssemblyID.ParameterName = "AssemblyID";
                        dpAssemblyID.Value = assembly.ID;
                        cmd.Parameters.Add(dpAssemblyID);

                        resultsID = (int)cmd.ExecuteScalar();
                    }

                    log.Info("Saving data to AnalyzeResults table.");
                    foreach (AnalyseOptions analyseOption in result.Result.Keys)
                    {
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "INSERT INTO AnalyzeResults(ResultsID,AnalyzeOptionID,Result) " +
                                                "VALUES(@ResultsID,@AnalyzeOptionID,@Result)";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultsID;
                            cmd.Parameters.Add(dpResultsID);

                            DbParameter dpAnalyseOptionID = provider.CreateParameter();
                            dpAnalyseOptionID.ParameterName = "AnalyzeOptionID";
                            dpAnalyseOptionID.Value = Convert.ToInt32(analyseOption);
                            cmd.Parameters.Add(dpAnalyseOptionID);

                            DbParameter dpResult = provider.CreateParameter();
                            dpResult.ParameterName = "Result";
                            dpResult.Value = result.Result[analyseOption];
                            cmd.Parameters.Add(dpResult);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    SqlBulkCopy cp = new SqlBulkCopy(GetConnectionString(),
                        SqlBulkCopyOptions.CheckConstraints);

                    log.Info("Saving data to VertexDegree table.");
                    DataTable vertexDegreeTable = new DataTable("VertexDegree");
                    vertexDegreeTable.Columns.Add("ResultsID", typeof(Int32));
                    vertexDegreeTable.Columns.Add("Degree", typeof(Int32));
                    vertexDegreeTable.Columns.Add("Count", typeof(Int32));

                    foreach (int degree in result.VertexDegree.Keys)
                    {
                        DataRow r = vertexDegreeTable.NewRow();
                        r[0] = resultsID;
                        r[1] = degree;
                        r[2] = result.VertexDegree[degree];
                        vertexDegreeTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "VertexDegree";
                    cp.WriteToServer(vertexDegreeTable);

                    log.Info("Saving data to Coefficients table.");
                    DataTable coefficientsTable = new DataTable("Coefficients");
                    coefficientsTable.Columns.Add("ResultsID", typeof(Int32));
                    coefficientsTable.Columns.Add("Coefficient", typeof(Double));
                    coefficientsTable.Columns.Add("Count", typeof(Int32));

                    foreach (double coefficient in result.Coefficient.Keys)
                    {
                        DataRow r = coefficientsTable.NewRow();
                        r[0] = resultsID;
                        r[1] = coefficient;
                        r[2] = result.Coefficient[coefficient];
                        coefficientsTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "Coefficients";
                    cp.WriteToServer(coefficientsTable);

                    log.Info("Saving data to ConSubgraphs table.");
                    DataTable conSubgraphsTable = new DataTable("ConSubgraphs");
                    conSubgraphsTable.Columns.Add("ResultsID", typeof(Int32));
                    conSubgraphsTable.Columns.Add("VX", typeof(Int32));
                    conSubgraphsTable.Columns.Add("Count", typeof(Int32));

                    foreach (int subgraph in result.Subgraphs.Keys)
                    {
                        DataRow r = conSubgraphsTable.NewRow();
                        r[0] = resultsID;
                        r[1] = subgraph;
                        r[2] = result.Subgraphs[subgraph];
                        conSubgraphsTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "ConSubgraphs";
                    cp.WriteToServer(conSubgraphsTable);

                    log.Info("Saving data to FullSubgraphs table.");
                    DataTable fullSubgraphsTable = new DataTable("FullSubgraphs");
                    fullSubgraphsTable.Columns.Add("ResultsID", typeof(Int32));
                    fullSubgraphsTable.Columns.Add("VX", typeof(Int32));
                    fullSubgraphsTable.Columns.Add("Count", typeof(Int32));

                    foreach (int subgraph in result.FullSubgraphs.Keys)
                    {
                        DataRow r = fullSubgraphsTable.NewRow();
                        r[0] = resultsID;
                        r[1] = subgraph;
                        r[2] = result.FullSubgraphs[subgraph];
                        fullSubgraphsTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "FullSubgraphs";
                    cp.WriteToServer(fullSubgraphsTable);

                    log.Info("Saving data to VertexDistance table.");
                    DataTable vertexDistanceTable = new DataTable("VertexDistance");
                    vertexDistanceTable.Columns.Add("ResultsID", typeof(Int32));
                    vertexDistanceTable.Columns.Add("Distance", typeof(Int32));
                    vertexDistanceTable.Columns.Add("Count", typeof(Int32));

                    foreach (int dist in result.DistanceBetweenVertices.Keys)
                    {
                        DataRow r = vertexDistanceTable.NewRow();
                        r[0] = resultsID;
                        r[1] = dist;
                        r[2] = result.DistanceBetweenVertices[dist];
                        vertexDistanceTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "VertexDistance";
                    cp.WriteToServer(vertexDistanceTable);

                    log.Info("Saving data to EigenValues table.");
                    DataTable eigenValuesTable = new DataTable("EigenValues");
                    eigenValuesTable.Columns.Add("ResultsID", typeof(Int32));
                    eigenValuesTable.Columns.Add("EigenValue", typeof(Double));

                    foreach (double value in result.EigenVector)
                    {
                        DataRow r = eigenValuesTable.NewRow();
                        r[0] = resultsID;
                        r[1] = value;
                        eigenValuesTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "EigenValues";
                    cp.WriteToServer(eigenValuesTable);

                    log.Info("Saving data to EigenValuesDistance table.");
                    DataTable eigenValuesDistanceTable = new DataTable("EigenValuesDistance");
                    eigenValuesDistanceTable.Columns.Add("ResultsID", typeof(Int32));
                    eigenValuesDistanceTable.Columns.Add("Distance", typeof(Double));
                    eigenValuesDistanceTable.Columns.Add("Count", typeof(Int32));

                    foreach (double dist in result.DistancesBetweenEigenValues.Keys)
                    {
                        DataRow r = eigenValuesDistanceTable.NewRow();
                        r[0] = resultsID;
                        r[1] = dist;
                        r[2] = result.DistancesBetweenEigenValues[dist];
                        eigenValuesDistanceTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "EigenValuesDistance";
                    cp.WriteToServer(eigenValuesDistanceTable);

                    log.Info("Saving data to Cycles table.");
                    DataTable cyclesTable = new DataTable("Cycles");
                    cyclesTable.Columns.Add("ResultsID", typeof(Int32));
                    cyclesTable.Columns.Add("Order", typeof(Int32));
                    cyclesTable.Columns.Add("Count", typeof(long));

                    foreach (int order in result.Cycles.Keys)
                    {
                        DataRow r = cyclesTable.NewRow();
                        r[0] = resultsID;
                        r[1] = order;
                        r[2] = result.Cycles[order];
                        cyclesTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "Cycles";
                    cp.WriteToServer(cyclesTable);

                    log.Info("Saving data to Triangles table.");
                    DataTable trianglesTable = new DataTable("Triangles");
                    trianglesTable.Columns.Add("ResultsID", typeof(Int32));
                    trianglesTable.Columns.Add("TriangleCount", typeof(Int32));
                    trianglesTable.Columns.Add("Count", typeof(Int32));

                    foreach (int tr in result.TriangleCount.Keys)
                    {
                        DataRow r = trianglesTable.NewRow();
                        r[0] = resultsID;
                        r[1] = tr;
                        r[2] = result.TriangleCount[tr];
                        trianglesTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "Triangles";
                    cp.WriteToServer(trianglesTable);

                    log.Info("Saving data to TriangleTrajectory table.");
                    DataTable triangleTrajectoryTable = new DataTable("TriangleTrajectory");
                    triangleTrajectoryTable.Columns.Add("ResultsID", typeof(Int32));
                    triangleTrajectoryTable.Columns.Add("Time", typeof(Int32));
                    triangleTrajectoryTable.Columns.Add("TriangleCount", typeof(Double));

                    foreach (int time in result.TriangleTrajectory.Keys)
                    {
                        DataRow r = triangleTrajectoryTable.NewRow();
                        r[0] = resultsID;
                        r[1] = time;
                        r[2] = result.TriangleTrajectory[time];
                        triangleTrajectoryTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "TriangleTrajectory";
                    cp.WriteToServer(triangleTrajectoryTable);

                    log.Info("Saving data to Motifs table.");
                    DataTable motifsTable = new DataTable("Motifs");
                    motifsTable.Columns.Add("ResultsID", typeof(Int32));
                    motifsTable.Columns.Add("ID", typeof(Int32));
                    motifsTable.Columns.Add("Count", typeof(float));

                    foreach (int id in result.MotivesCount.Keys)
                    {
                        DataRow r = motifsTable.NewRow();
                        r[0] = resultsID;
                        r[1] = id;
                        r[2] = result.MotivesCount[id];
                        motifsTable.Rows.Add(r);
                    }

                    cp.DestinationTableName = "Motifs";
                    cp.WriteToServer(motifsTable);

                    cp.Close();
                }
            }
        }

        // Удаление сборки по данному идентификатору сборки.
        public override void Delete(Guid assemblyID)
        {
            log.Info("Deleting data from DB.");
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                conn.Open();
                using (DbCommand cmd = conn.CreateCommand())
                {
                    string sqlQuery = 
                        "DELETE FROM AnalyzeResults WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM VertexDegree WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM Coefficients WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM ConSubgraphs WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM FullSubgraphs WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM VertexDistance WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM EigenValues WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM EigenValuesDistance WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM Cycles WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM Triangles WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM TriangleTrajectory WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM Motifs WHERE ResultsID IN (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID) " +
                        "DELETE FROM AssemblyResults WHERE AssemblyID=@AssemblyID " +
                        "DELETE FROM GenerationParamValues WHERE AssemblyID=@AssemblyID " +
                        "DELETE FROM AnalyzeOptionParamValues WHERE AssemblyID=@AssemblyID " +
                        "DELETE FROM Assemblies WHERE AssemblyID=@AssemblyID";
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    DbParameter dpAssemblyID = provider.CreateParameter();
                    dpAssemblyID.ParameterName = "AssemblyID";
                    dpAssemblyID.Value = assemblyID;
                    cmd.Parameters.Add(dpAssemblyID);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Загрузка сборки по данному идентификатору сборки.
        public override ResultAssembly Load(Guid assemblyID)
        {
            log.Info("Loading assembly with ID " + assemblyID.ToString() + ".");
            
            ResultAssembly resultAssembly = new ResultAssembly();
            resultAssembly.ID = assemblyID;

            log.Info("Openning connection.");
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                try
                {
                    conn.Open();
                }
                catch (Exception)
                {
                    log.Info("Could not open a connection with DB.");
                    return new ResultAssembly();
                }

                log.Info("Loading data from tables Assemblies, GenerationParamValues, AnalyzeOptionParamValues.");
                using (DbCommand cmd = conn.CreateCommand())
                {
                    string sqlQuery = "SELECT Assemblies.[Name], Assemblies.ModelID, Assemblies.NetworkSize, Assemblies.FileName,GenerationParamValues.* FROM Assemblies " +
                                        "LEFT JOIN GenerationParamValues ON GenerationParamValues.AssemblyID=Assemblies.AssemblyID " +
                                        "WHERE Assemblies.AssemblyID=@AssemblyID ORDER BY GenerationParamID";
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    DbParameter dpID = provider.CreateParameter();
                    dpID.ParameterName = "AssemblyID";
                    dpID.Value = assemblyID;
                    cmd.Parameters.Add(dpID);

                    using (DbDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            resultAssembly.ModelType = GetModelType((int)dr["ModelID"]);
                            resultAssembly.Name = (string)dr["Name"];
                            resultAssembly.Size = (int)dr["NetworkSize"];
                            resultAssembly.FileName = (string)dr["FileName"];

                            GenerationParam param = (GenerationParam)Enum.ToObject(typeof(GenerationParam), (int)dr["GenerationParamID"]);

                            GenerationParamInfo paramInfo = (GenerationParamInfo)(param.GetType().GetField(param.ToString()).GetCustomAttributes(typeof(GenerationParamInfo), false)[0]);
                            if (paramInfo.Type.Equals(typeof(Double)))
                            {
                                resultAssembly.GenerationParams.Add(param, Convert.ToDouble(dr["Value"], CultureInfo.InvariantCulture));
                            }
                            else if (paramInfo.Type.Equals(typeof(Int16)))
                            {
                                resultAssembly.GenerationParams.Add(param, Convert.ToInt16(dr["Value"]));
                            }
                            else if (paramInfo.Type.Equals(typeof(Int32)))
                            {
                                resultAssembly.GenerationParams.Add(param, Convert.ToInt32(dr["Value"]));
                            }
                            else if (paramInfo.Type.Equals(typeof(bool)))
                            {
                                resultAssembly.GenerationParams.Add(param, Convert.ToBoolean(dr["Value"]));
                            }
                            else if (paramInfo.Type.Equals(typeof(String)))
                            {
                                resultAssembly.GenerationParams.Add(param, Convert.ToString(dr["Value"]));
                            }
                        }
                    }

                    sqlQuery = "SELECT Assemblies.[Name],AnalyzeOptionParamValues.* FROM Assemblies " +
                        "LEFT JOIN AnalyzeOptionParamValues ON AnalyzeOptionParamValues.AssemblyID=Assemblies.AssemblyID " +
                        "WHERE Assemblies.AssemblyID=@AssemblyID ORDER BY AnalyzeOptionParamID";
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    using (DbDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            AnalyzeOptionParam param = (AnalyzeOptionParam)Enum.ToObject(typeof(AnalyzeOptionParam),
                                (int)dr["AnalyzeOptionParamID"]);

                            AnalyzeOptionParamInfo paramInfo = (AnalyzeOptionParamInfo)(param.GetType().GetField(param.ToString()).GetCustomAttributes(typeof(AnalyzeOptionParamInfo), false)[0]);
                            if (paramInfo.Type.Equals(typeof(Double)))
                            {
                                resultAssembly.AnalyzeOptionParams[param] = Convert.ToDouble(dr["Value"], 
                                    CultureInfo.InvariantCulture);
                            }
                            else if (paramInfo.Type.Equals(typeof(Int16)))
                            {
                                resultAssembly.AnalyzeOptionParams[param] = Convert.ToInt16(dr["Value"]);
                            }
                            else if (paramInfo.Type.Equals(typeof(BigInteger)))
                            {
                                resultAssembly.AnalyzeOptionParams[param] = Convert.ToString(dr["Value"]);
                            }
                        }
                    }
                }

                log.Info("Loading analyze results data.");
                using (DbCommand mainCmd = conn.CreateCommand())
                {
                    string sql = "SELECT ResultsID FROM AssemblyResults WHERE AssemblyID=@AssemblyID";
                    mainCmd.CommandText = sql;
                    mainCmd.CommandType = CommandType.Text;

                    DbParameter dpID = provider.CreateParameter();
                    dpID.ParameterName = "AssemblyID";
                    dpID.Value = assemblyID;
                    mainCmd.Parameters.Add(dpID);

                    List<int> resultIDs = new List<int>();
                    using (DbDataReader mainReader = mainCmd.ExecuteReader())
                    {
                        while (mainReader.Read())
                        {
                            resultIDs.Add((int)mainReader["ResultsID"]);
                        }
                    }

                    foreach (int resultID in resultIDs)
	                {
	                    AnalizeResult result = new AnalizeResult();
                        resultAssembly.Results.Add(result);

                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM AnalyzeResults WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.Result.Add((AnalyseOptions)Enum.ToObject(typeof(AnalyseOptions), dr["AnalyzeOptionID"]), Convert.ToDouble(dr["Result"]));
                                }
                            }
                        }

                        log.Info("Loading data from table VertexDegree.");
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM VertexDegree WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.VertexDegree.Add((int)dr["Degree"], (int)dr["Count"]);
                                }
                            }
                        }

                        log.Info("Loading data from table Coefficients.");
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM Coefficients  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.Coefficient.Add(Convert.ToDouble(dr["Coefficient"]), (int)dr["Count"]);
                                }
                            }
                        }

                        log.Info("Loading data from table ConSubgraphs.");
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM ConSubgraphs  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.Subgraphs.Add(Convert.ToInt32(dr["VX"]), (int)dr["Count"]);
                                }
                            }
                        }

                        log.Info("Loading data from table FullSubgraphs.");
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM FullSubgraphs  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.FullSubgraphs.Add(Convert.ToInt32(dr["VX"]), (int)dr["Count"]);
                                }
                            }
                        }

                        log.Info("Loading data from table VertexDistance.");
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM VertexDistance  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.DistanceBetweenVertices.Add(Convert.ToInt32(dr["Distance"]), (int)dr["Count"]);
                                }
                            }
                        }

                        log.Info("Loading data from table EigenValue.");    // !проверить!
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM EigenValues  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.EigenVector.Add(dr["EigenValue"]);
                                }
                            }
                        }

                        log.Info("Loading data from table EigenValuesDistance.");
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM EigenValuesDistance  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.DistancesBetweenEigenValues.Add(Convert.ToDouble(dr["Distance"]), (int)dr["Count"]);
                                }
                            }
                        }

                        log.Info("Loading data from table Cycles.");    // !проверить!
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM Cycles  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.Cycles.Add(Convert.ToInt32(dr["Order"]), (long)dr["Count"]);
                                }
                            }
                        }

                        log.Info("Loading data from table Triangles.");
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM Triangles  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.TriangleCount.Add(Convert.ToInt32(dr["TriangleCount"]), (int)dr["Count"]);
                                }
                            }
                        }

                        log.Info("Loading data from table TriangleTrajectory.");
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM TriangleTrajectory  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.TriangleTrajectory.Add(Convert.ToInt32(dr["Time"]), (double)dr["TriangleCount"]);
                                }
                            }
                        }

                        log.Info("Loading data from table Motifs.");
                        using (DbCommand cmd = conn.CreateCommand())
                        {
                            string sqlQuery = "SELECT * FROM Motifs  WHERE ResultsID=@ResultsID";
                            cmd.CommandText = sqlQuery;
                            cmd.CommandType = CommandType.Text;

                            DbParameter dpResultsID = provider.CreateParameter();
                            dpResultsID.ParameterName = "ResultsID";
                            dpResultsID.Value = resultID;
                            cmd.Parameters.Add(dpResultsID);

                            using (DbDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    result.MotivesCount.Add(Convert.ToInt32(dr["ID"]), float.Parse(dr["Count"].ToString()));
                                }
                            }
                        }
                    }
                }
            }

            return resultAssembly;
        }

        // Загрузка всех сборок.
        public override List<ResultAssembly> LoadAllAssemblies()
        {
            List<ResultAssembly> results = new List<ResultAssembly>();
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                conn.Open();
                ResultAssembly result = null;
                using (DbCommand cmd = conn.CreateCommand())
                {
                    string sqlQuery = "SELECT * FROM Assemblies";
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;

                    using (DbDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result = new ResultAssembly();
                            results.Add(result);
                            result.ID = (Guid) dr["AssemblyID"];
                            result.ModelType = GetModelType((int)dr["ModelID"]);
                            result.ModelName = result.ModelType.Name;
                            result.Name = (string)dr["Name"];
                            result.Size = (int)dr["NetworkSize"];
                        }
                    }
                }
            }
            return results;
        }

        #endregion
    }
}
