/****** Object:  StoredProcedure [dbo].[CountGlobalCoefficients]    Script Date: 04/12/2013 10:17:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Ani Kocharyan="">
  -- Create date: <06.04.13>
    -- Description:	<Counting Global="" Coefficients="" for="" one="" realization="">
      -- =============================================
      CREATE PROCEDURE [dbo].[CountGlobalCoefficients]
      @ResID int = 0,
      @NetworkSize int = 1
      AS
      BEGIN
      SET NOCOUNT ON;

      DECLARE @avgCoefficient float;
      SELECT @avgCoefficient = SUM([Sum])/ @NetworkSize
      FROM
      (
      SELECT Coefficient * [Count] AS [Sum] FROM Coefficients WHERE ResultsID = @ResID
      )
      AS TEST

      INSERT INTO CoefficientsGlobal(ResultsID,AvgCoefficient) VALUES(@ResID,@avgCoefficient)
      END

      GO
      /****** Object:  StoredProcedure [dbo].[CountAllGlobalCoefficients]    Script Date: 04/12/2013 10:18:07 ******/
      SET ANSI_NULLS ON
      GO
      SET QUOTED_IDENTIFIER ON
      GO
      -- =============================================
      -- Author:		<Ani Kocharyan="">
        -- Create date: <06.04.13>
          -- Description:	<Counting Global="" Coefficients="" for="" an="" assembly="" (for="" all="" realizations="")>
            -- =============================================
            CREATE PROCEDURE [dbo].[CountAllGlobalCoefficients]
            @AssID uniqueidentifier
            AS
            BEGIN
            SET NOCOUNT ON;

            DECLARE @paramID int;
            DECLARE @paramNetworkSize int;

            DECLARE ResCursor CURSOR FOR
            SELECT ResultsID, NetworkSize FROM AssemblyResults WHERE AssemblyID = @AssID;

            OPEN ResCursor;
            FETCH FROM ResCursor INTO @paramID, @paramNetworkSize;
            WHILE @@FETCH_STATUS = 0
            BEGIN
            EXEC CountGlobalCoefficients @paramID, @paramNetworkSize;
            FETCH NEXT FROM ResCursor INTO @paramID, @paramNetworkSize;
            END;
            CLOSE ResCursor;
            DEALLOCATE ResCursor;
            END

            GO
            /****** Object:  StoredProcedure [dbo].[CountGlobalVertexDegrees]    Script Date: 04/12/2013 10:18:30 ******/
            SET ANSI_NULLS ON
            GO
            SET QUOTED_IDENTIFIER ON
            GO
            -- =============================================
            -- Author:		<Ani Kocharyan="">
              -- Create date: <08.04.13>
                -- Description:	<Counting Global="" Degree="" Distributions="" for="" one="" realization="">
                  -- =============================================
                  CREATE PROCEDURE [dbo].[CountGlobalVertexDegrees]
                  @ResID int = 0,
                  @NetworkSize int = 1
                  AS
                  BEGIN
                  SET NOCOUNT ON;

                  DECLARE @avgDegree float;
                  SELECT @avgDegree = SUM([Sum])/ @NetworkSize
                  FROM
                  (
                  SELECT CAST(Degree AS float) * CAST([Count] AS float) AS [Sum] FROM VertexDegree WHERE ResultsID = @ResID
                  )
                  AS TEST

                  INSERT INTO VertexDegreeGlobal(ResultsID,AvgDegree) VALUES(@ResID,@avgDegree)
                  END

                  GO
                  /****** Object:  StoredProcedure [dbo].[CountAllGlobalVertexDegrees]    Script Date: 04/12/2013 10:18:46 ******/
                  SET ANSI_NULLS ON
                  GO
                  SET QUOTED_IDENTIFIER ON
                  GO
                  -- =============================================
                  -- Author:		<Ani Kocharyan="">
                    -- Create date: <08.04.13>
                      -- Description:	<Counting Global="" Degree="" Distributions="" for="" an="" assembly="" (for="" all="" realizations="")>
                        -- =============================================
                        CREATE PROCEDURE [dbo].[CountAllGlobalVertexDegrees]
                        @AssID uniqueidentifier
                        AS
                        BEGIN
                        SET NOCOUNT ON;

                        DECLARE @paramID int;
                        DECLARE @paramNetworkSize int;

                        DECLARE ResCursor CURSOR FOR
                        SELECT ResultsID, NetworkSize FROM AssemblyResults WHERE AssemblyID = @AssID;

                        OPEN ResCursor;
                        FETCH FROM ResCursor INTO @paramID, @paramNetworkSize;
                        WHILE @@FETCH_STATUS = 0
                        BEGIN
                        EXEC CountGlobalVertexDegrees @paramID, @paramNetworkSize;
                        FETCH NEXT FROM ResCursor INTO @paramID, @paramNetworkSize;
                        END;
                        CLOSE ResCursor;
                        DEALLOCATE ResCursor;
                        END

                        GO
                        /****** Object:  StoredProcedure [dbo].[CountLocalCoefficients]    Script Date: 04/12/2013 10:19:33 ******/
                        SET ANSI_NULLS ON
                        GO
                        SET QUOTED_IDENTIFIER ON
                        GO
                        -- =============================================
                        -- Author:		<Ani Kocharyan="">
                          -- Create date: <08.04.13>
                            -- Description:	<Counting Local="" Coefficients="" for="" one="" realization="">
                              -- =============================================
                              CREATE PROCEDURE [dbo].[CountLocalCoefficients]
                              @AssID uniqueidentifier,
                              @ResID int = 0,
                              @NetworkSize int = 1
                              AS
                              BEGIN
                              SET NOCOUNT ON;

                              DECLARE CoeffCursor CURSOR FOR
                              SELECT Coefficient, [Count] FROM Coefficients WHERE ResultsID = @ResID;

                              DECLARE @key float;
                              DECLARE @value int;
                              DECLARE @valueToAdd float;

                              OPEN CoeffCursor;
                              FETCH FROM CoeffCursor INTO @key,@value;
                              WHILE @@FETCH_STATUS = 0
                              BEGIN
                              SET @valueToAdd = CAST(@value AS FLOAT)/CAST(@NetworkSize AS FLOAT);

                              IF EXISTS(SELECT * FROM CoefficientsLocal WHERE AssemblyID = @AssID AND Coefficient = @key)
                              BEGIN
                              UPDATE CoefficientsLocal SET Distribution = Distribution + @valueToAdd
                              WHERE AssemblyID = @AssID AND Coefficient = @key
                              END
                              ELSE
                              BEGIN
                              INSERT INTO CoefficientsLocal(AssemblyID,Coefficient,Distribution) VALUES(@AssID,@key,@valueToAdd)
                              END

                              FETCH NEXT FROM CoeffCursor INTO @key,@value;
                              END;
                              CLOSE CoeffCursor;
                              DEALLOCATE CoeffCursor;
                              END

                              GO
                              /****** Object:  StoredProcedure [dbo].[CountAllLocalCoefficients]    Script Date: 04/12/2013 10:19:45 ******/
                              SET ANSI_NULLS ON
                              GO
                              SET QUOTED_IDENTIFIER ON
                              GO
                              -- =============================================
                              -- Author:		<Ani Kocharyan="">
                                -- Create date: <08.04.13>
                                  -- Description:	<Counting Local="" Coefficients="" for="" an="" assembly="" (for="" all="" realizations="")>
                                    -- =============================================
                                    CREATE PROCEDURE [dbo].[CountAllLocalCoefficients]
                                    @AssID uniqueidentifier
                                    AS
                                    BEGIN
                                    SET NOCOUNT ON;

                                    DECLARE @paramID int;
                                    DECLARE @paramNetworkSize int;

                                    DECLARE ResCursor CURSOR FOR
                                    SELECT ResultsID, NetworkSize FROM AssemblyResults WHERE AssemblyID = @AssID;

                                    OPEN ResCursor;
                                    FETCH FROM ResCursor INTO @paramID, @paramNetworkSize;
                                    WHILE @@FETCH_STATUS = 0
                                    BEGIN
                                    EXEC CountLocalCoefficients @AssID,@paramID,@paramNetworkSize;
                                    FETCH NEXT FROM ResCursor INTO @paramID, @paramNetworkSize;
                                    END;
                                    CLOSE ResCursor;
                                    DEALLOCATE ResCursor;

                                    DECLARE @CountOfRealizations int;
                                    SELECT @CountOfRealizations = COUNT(*) FROM AssemblyResults WHERE AssemblyID = @AssID;

                                    UPDATE CoefficientsLocal SET Distribution = Distribution / @CountOfRealizations
                                    WHERE AssemblyID = @AssID;
                                    END

                                    GO
                                    /****** Object:  StoredProcedure [dbo].[CountLocalVertexDegrees]    Script Date: 04/12/2013 10:21:47 ******/
                                    SET ANSI_NULLS ON
                                    GO
                                    SET QUOTED_IDENTIFIER ON
                                    GO
                                    -- =============================================
                                    -- Author:		<Ani Kocharyan="">
                                      -- Create date: <08.04.13>
                                        -- Description:	<Counting Local="" Vertex="" Degrees="" for="" one="" realization="">
                                          -- =============================================
                                          CREATE PROCEDURE [dbo].[CountLocalVertexDegrees]
                                          @AssID uniqueidentifier,
                                          @ResID int = 0,
                                          @NetworkSize int = 1
                                          AS
                                          BEGIN
                                          SET NOCOUNT ON;

                                          DECLARE VertCursor CURSOR FOR
                                          SELECT Degree, [Count] FROM VertexDegree WHERE ResultsID = @ResID;

                                          DECLARE @key int;
                                          DECLARE @value int;
                                          DECLARE @valueToAdd float;

                                          OPEN VertCursor;
                                          FETCH FROM VertCursor INTO @key,@value;
                                          WHILE @@FETCH_STATUS = 0
                                          BEGIN
                                          SET @valueToAdd = CAST(@value AS FLOAT)/CAST(@NetworkSize AS FLOAT);

                                          IF EXISTS(SELECT * FROM VertexDegreeLocal WHERE AssemblyID = @AssID AND Degree = @key)
                                          BEGIN
                                          UPDATE VertexDegreeLocal SET Distribution = Distribution + @valueToAdd
                                          WHERE AssemblyID = @AssID AND Degree = @key
                                          END
                                          ELSE
                                          BEGIN
                                          INSERT INTO VertexDegreeLocal(AssemblyID,Degree,Distribution) VALUES(@AssID,@key,@valueToAdd)
                                          END

                                          FETCH NEXT FROM VertCursor INTO @key,@value;
                                          END;
                                          CLOSE VertCursor;
                                          DEALLOCATE VertCursor;
                                          END

                                          GO
                                          /****** Object:  StoredProcedure [dbo].[CountAllLocalVertexDegrees]    Script Date: 04/12/2013 10:21:58 ******/
                                          SET ANSI_NULLS ON
                                          GO
                                          SET QUOTED_IDENTIFIER ON
                                          GO
                                          -- =============================================
                                          -- Author:		<Ani Kocharyan="">
                                            -- Create date: <08.04.13>
                                              -- Description:	<Counting Local="" Vertex="" Degrees="" for="" an="" assembly="" (for="" all="" realizations="")>
                                                -- =============================================
                                                CREATE PROCEDURE [dbo].[CountAllLocalVertexDegrees]
                                                @AssID uniqueidentifier
                                                AS
                                                BEGIN
                                                SET NOCOUNT ON;

                                                DECLARE @paramID int;
                                                DECLARE @paramNetworkSize int;

                                                DECLARE ResCursor CURSOR FOR
                                                SELECT ResultsID, NetworkSize FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                OPEN ResCursor;
                                                FETCH FROM ResCursor INTO @paramID, @paramNetworkSize;
                                                WHILE @@FETCH_STATUS = 0
                                                BEGIN
                                                EXEC CountLocalVertexDegrees @AssID,@paramID,@paramNetworkSize;
                                                FETCH NEXT FROM ResCursor INTO @paramID, @paramNetworkSize;
                                                END;
                                                CLOSE ResCursor;
                                                DEALLOCATE ResCursor;

                                                DECLARE @CountOfRealizations int;
                                                SELECT @CountOfRealizations = COUNT(*) FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                UPDATE VertexDegreeLocal SET Distribution = Distribution / @CountOfRealizations
                                                WHERE AssemblyID = @AssID;
                                                END

                                                GO
                                                /****** Object:  StoredProcedure [dbo].[CountLocalVertexDistances]    Script Date: 04/12/2013 10:22:11 ******/
                                                SET ANSI_NULLS ON
                                                GO
                                                SET QUOTED_IDENTIFIER ON
                                                GO
                                                -- =============================================
                                                -- Author:		<Ani Kocharyan="">
                                                  -- Create date: <09.04.13>
                                                    -- Description:	<Counting Local="" Vertex="" Distance="" for="" one="" realization="">
                                                      -- =============================================
                                                      CREATE PROCEDURE [dbo].[CountLocalVertexDistances]
                                                      @AssID uniqueidentifier,
                                                      @ResID int = 0,
                                                      @Div int = 1
                                                      AS
                                                      BEGIN
                                                      SET NOCOUNT ON;

                                                      DECLARE DistCursor CURSOR FOR
                                                      SELECT Distance, [Count] FROM VertexDistance WHERE ResultsID = @ResID;

                                                      DECLARE @key int;
                                                      DECLARE @value int;
                                                      DECLARE @valueToAdd float;

                                                      OPEN DistCursor;
                                                      FETCH FROM DistCursor INTO @key,@value;
                                                      WHILE @@FETCH_STATUS = 0
                                                      BEGIN
                                                      SET @valueToAdd = CAST(@value AS FLOAT)/CAST(@Div AS FLOAT);

                                                      IF EXISTS(SELECT * FROM VertexDistanceLocal WHERE AssemblyID = @AssID AND Distance = @key)
                                                      BEGIN
                                                      UPDATE VertexDistanceLocal SET Distribution = Distribution + @valueToAdd
                                                      WHERE AssemblyID = @AssID AND Distance = @key
                                                      END
                                                      ELSE
                                                      BEGIN
                                                      INSERT INTO VertexDistanceLocal(AssemblyID,Distance,Distribution) VALUES(@AssID,@key,@valueToAdd)
                                                      END

                                                      FETCH NEXT FROM DistCursor INTO @key,@value;
                                                      END;
                                                      CLOSE DistCursor;
                                                      DEALLOCATE DistCursor;
                                                      END

                                                      GO
                                                      /****** Object:  StoredProcedure [dbo].[CountAllLocalVertexDistances]    Script Date: 04/12/2013 10:22:23 ******/
                                                      SET ANSI_NULLS ON
                                                      GO
                                                      SET QUOTED_IDENTIFIER ON
                                                      GO
                                                      -- =============================================
                                                      -- Author:		<Ani Kocharyan="">
                                                        -- Create date: <09.04.13>
                                                          -- Description:	<Counting Local="" Vertex="" Distances="" for="" an="" assembly="" (for="" all="" realizations="")>
                                                            -- =============================================
                                                            CREATE PROCEDURE [dbo].[CountAllLocalVertexDistances]
                                                            @AssID uniqueidentifier
                                                            AS
                                                            BEGIN
                                                            SET NOCOUNT ON;

                                                            DECLARE @paramID int;
                                                            DECLARE @paramNetworkSize int;
                                                            DECLARE @paramDiv int;

                                                            DECLARE ResCursor CURSOR FOR
                                                            SELECT ResultsID, NetworkSize FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                            OPEN ResCursor;
                                                            FETCH FROM ResCursor INTO @paramID, @paramNetworkSize;
                                                            WHILE @@FETCH_STATUS = 0
                                                            BEGIN
                                                            SET @paramDiv = @paramNetworkSize * (@paramNetworkSize - 1) / 2;
                                                            EXEC CountLocalVertexDistances @AssID,@paramID,@paramDiv;
                                                            FETCH NEXT FROM ResCursor INTO @paramID, @paramNetworkSize;
                                                            END;
                                                            CLOSE ResCursor;
                                                            DEALLOCATE ResCursor;

                                                            DECLARE @CountOfRealizations int;
                                                            SELECT @CountOfRealizations = COUNT(*) FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                            UPDATE VertexDistanceLocal SET Distribution = Distribution / @CountOfRealizations
                                                            WHERE AssemblyID = @AssID;
                                                            END

                                                            GO
                                                            /****** Object:  StoredProcedure [dbo].[CountLocalConSubgraphs]    Script Date: 04/12/2013 10:20:00 ******/
                                                            SET ANSI_NULLS ON
                                                            GO
                                                            SET QUOTED_IDENTIFIER ON
                                                            GO
                                                            -- =============================================
                                                            -- Author:		<Ani Kocharyan="">
                                                              -- Create date: <09.03.13>
                                                                -- Description:	<Counting Local="" Connected="" Subgraphs="" for="" one="" realization="">
                                                                  -- =============================================
                                                                  CREATE PROCEDURE [dbo].[CountLocalConSubgraphs]
                                                                  @AssID uniqueidentifier,
                                                                  @ResID int = 0,
                                                                  @NetworkSize int = 1
                                                                  AS
                                                                  BEGIN
                                                                  SET NOCOUNT ON;

                                                                  DECLARE ConCursor CURSOR FOR
                                                                  SELECT VX, [Count] FROM ConSubgraphs WHERE ResultsID = @ResID;

                                                                  DECLARE @key int;
                                                                  DECLARE @value int;
                                                                  DECLARE @valueToAdd float;

                                                                  OPEN ConCursor;
                                                                  FETCH FROM ConCursor INTO @key,@value;
                                                                  WHILE @@FETCH_STATUS = 0
                                                                  BEGIN
                                                                  SET @valueToAdd = CAST(@value AS FLOAT)/CAST(@NetworkSize AS FLOAT);

                                                                  IF EXISTS(SELECT * FROM ConSubgraphsLocal WHERE AssemblyID = @AssID AND VX = @key)
                                                                  BEGIN
                                                                  UPDATE ConSubgraphsLocal SET Distribution = Distribution + @valueToAdd
                                                                  WHERE AssemblyID = @AssID AND VX = @key
                                                                  END
                                                                  ELSE
                                                                  BEGIN
                                                                  INSERT INTO ConSubgraphsLocal(AssemblyID,VX,Distribution) VALUES(@AssID,@key,@valueToAdd)
                                                                  END

                                                                  FETCH NEXT FROM ConCursor INTO @key,@value;
                                                                  END;
                                                                  CLOSE ConCursor;
                                                                  DEALLOCATE ConCursor;
                                                                  END

                                                                  GO
                                                                  /****** Object:  StoredProcedure [dbo].[CountAllLocalConSubgraphs]    Script Date: 04/12/2013 10:20:13 ******/
                                                                  SET ANSI_NULLS ON
                                                                  GO
                                                                  SET QUOTED_IDENTIFIER ON
                                                                  GO
                                                                  -- =============================================
                                                                  -- Author:		<Ani Kocharyan="">
                                                                    -- Create date: <09.04.13>
                                                                      -- Description:	<Counting Local="" Connected="" Subgraphs="" for="" an="" assembly="" (for="" all="" realizations="")>
                                                                        -- =============================================
                                                                        CREATE PROCEDURE [dbo].[CountAllLocalConSubgraphs]
                                                                        @AssID uniqueidentifier
                                                                        AS
                                                                        BEGIN
                                                                        SET NOCOUNT ON;

                                                                        DECLARE @paramID int;
                                                                        DECLARE @paramNetworkSize int;

                                                                        DECLARE ResCursor CURSOR FOR
                                                                        SELECT ResultsID, NetworkSize FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                                        OPEN ResCursor;
                                                                        FETCH FROM ResCursor INTO @paramID, @paramNetworkSize;
                                                                        WHILE @@FETCH_STATUS = 0
                                                                        BEGIN
                                                                        EXEC CountLocalConSubgraphs @AssID,@paramID,@paramNetworkSize;
                                                                        FETCH NEXT FROM ResCursor INTO @paramID, @paramNetworkSize;
                                                                        END;
                                                                        CLOSE ResCursor;
                                                                        DEALLOCATE ResCursor;

                                                                        DECLARE @CountOfRealizations int;
                                                                        SELECT @CountOfRealizations = COUNT(*) FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                                        UPDATE ConSubgraphsLocal SET Distribution = Distribution / @CountOfRealizations
                                                                        WHERE AssemblyID = @AssID;
                                                                        END

                                                                        GO
                                                                        /****** Object:  StoredProcedure [dbo].[CountLocalEigenValuesDistances]    Script Date: 04/12/2013 10:20:40 ******/
                                                                        SET ANSI_NULLS ON
                                                                        GO
                                                                        SET QUOTED_IDENTIFIER ON
                                                                        GO
                                                                        -- =============================================
                                                                        -- Author:		<Ani Kocharyan="">
                                                                          -- Create date: <09.04.13>
                                                                            -- Description:	<Counting Local="" Eigen="" Values="" Distances="" for="" one="" realization="">
                                                                              -- =============================================
                                                                              CREATE PROCEDURE [dbo].[CountLocalEigenValuesDistances]
                                                                              @AssID uniqueidentifier,
                                                                              @ResID int = 0
                                                                              AS
                                                                              BEGIN
                                                                              SET NOCOUNT ON;

                                                                              DECLARE @div int;
                                                                              SELECT @div = COUNT(*) FROM EigenValuesDistance WHERE ResultsID = @ResID;

                                                                              DECLARE DistCursor CURSOR FOR
                                                                              SELECT Distance, [Count] FROM EigenValuesDistance WHERE ResultsID = @ResID;

                                                                              DECLARE @key float;
                                                                              DECLARE @value int;
                                                                              DECLARE @valueToAdd float;

                                                                              OPEN DistCursor;
                                                                              FETCH FROM DistCursor INTO @key,@value;
                                                                              WHILE @@FETCH_STATUS = 0
                                                                              BEGIN
                                                                              SET @valueToAdd = CAST(@value AS FLOAT)/CAST(@div AS FLOAT);

                                                                              IF EXISTS(SELECT * FROM EigenValuesDistanceLocal WHERE AssemblyID = @AssID AND Distance = @key)
                                                                              BEGIN
                                                                              UPDATE EigenValuesDistanceLocal SET Distribution = Distribution + @valueToAdd
                                                                              WHERE AssemblyID = @AssID AND Distance = @key
                                                                              END
                                                                              ELSE
                                                                              BEGIN
                                                                              INSERT INTO EigenValuesDistanceLocal(AssemblyID,Distance,Distribution) VALUES(@AssID,@key,@valueToAdd)
                                                                              END

                                                                              FETCH NEXT FROM DistCursor INTO @key,@value;
                                                                              END;
                                                                              CLOSE DistCursor;
                                                                              DEALLOCATE DistCursor;
                                                                              END

                                                                              GO
                                                                              /****** Object:  StoredProcedure [dbo].[CountAllLocalEigenValuesDistances]    Script Date: 04/12/2013 10:20:55 ******/
                                                                              SET ANSI_NULLS ON
                                                                              GO
                                                                              SET QUOTED_IDENTIFIER ON
                                                                              GO
                                                                              -- =============================================
                                                                              -- Author:		<Ani Kocharyan="">
                                                                                -- Create date: <09.04.13>
                                                                                  -- Description:	<Counting Local="" Eigen="" Values="" Distances="" for="" an="" assembly="" (for="" all="" realizations="")>
                                                                                    -- =============================================
                                                                                    CREATE PROCEDURE [dbo].[CountAllLocalEigenValuesDistances]
                                                                                    @AssID uniqueidentifier
                                                                                    AS
                                                                                    BEGIN
                                                                                    SET NOCOUNT ON;

                                                                                    DECLARE @paramID int;

                                                                                    DECLARE ResCursor CURSOR FOR
                                                                                    SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                                                    OPEN ResCursor;
                                                                                    FETCH FROM ResCursor INTO @paramID;
                                                                                    WHILE @@FETCH_STATUS = 0
                                                                                    BEGIN
                                                                                    EXEC CountLocalEigenValuesDistances @AssID,@paramID;
                                                                                    FETCH NEXT FROM ResCursor INTO @paramID;
                                                                                    END;
                                                                                    CLOSE ResCursor;
                                                                                    DEALLOCATE ResCursor;

                                                                                    DECLARE @CountOfRealizations int;
                                                                                    SELECT @CountOfRealizations = COUNT(*) FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                                                    UPDATE EigenValuesDistanceLocal SET Distribution = Distribution / @CountOfRealizations
                                                                                    WHERE AssemblyID = @AssID;
                                                                                    END

                                                                                    GO
                                                                                    /****** Object:  StoredProcedure [dbo].[CountLocalTriangleTrajectories]    Script Date: 04/12/2013 10:21:20 ******/
                                                                                    SET ANSI_NULLS ON
                                                                                    GO
                                                                                    SET QUOTED_IDENTIFIER ON
                                                                                    GO
                                                                                    -- =============================================
                                                                                    -- Author:		<Ani Kocharyan="">
                                                                                      -- Create date: <09.04.13>
                                                                                        -- Description:	<Counting Local="" Triangle="" Trajectories="" for="" one="" realization="">
                                                                                          -- =============================================
                                                                                          CREATE PROCEDURE [dbo].[CountLocalTriangleTrajectories]
                                                                                          @AssID uniqueidentifier,
                                                                                          @ResID int = 0
                                                                                          AS
                                                                                          BEGIN
                                                                                          SET NOCOUNT ON;

                                                                                          DECLARE TriangleCursor CURSOR FOR
                                                                                          SELECT [Time], TriangleCount FROM TriangleTrajectory WHERE ResultsID = @ResID;

                                                                                          DECLARE @key int;
                                                                                          DECLARE @value int;
                                                                                          DECLARE @valueToAdd float;

                                                                                          OPEN TriangleCursor;
                                                                                          FETCH FROM TriangleCursor INTO @key,@value;
                                                                                          WHILE @@FETCH_STATUS = 0
                                                                                          BEGIN
                                                                                          SET @valueToAdd = CAST(@value AS FLOAT);

                                                                                          IF EXISTS(SELECT * FROM TriangleTrajectoryLocal WHERE AssemblyID = @AssID AND [Time] = @key)
                                                                                          BEGIN
                                                                                          UPDATE TriangleTrajectoryLocal SET Distribution = Distribution + @valueToAdd
                                                                                          WHERE AssemblyID = @AssID AND [Time] = @key
                                                                                          END
                                                                                          ELSE
                                                                                          BEGIN
                                                                                          INSERT INTO TriangleTrajectoryLocal(AssemblyID,[Time],Distribution) VALUES(@AssID,@key,@valueToAdd)
                                                                                          END

                                                                                          FETCH NEXT FROM TriangleCursor INTO @key,@value;
                                                                                          END;
                                                                                          CLOSE TriangleCursor;
                                                                                          DEALLOCATE TriangleCursor;
                                                                                          END

                                                                                          GO
                                                                                          /****** Object:  StoredProcedure [dbo].[CountAllLocalTriangleTrajectories]    Script Date: 04/12/2013 10:21:32 ******/
                                                                                          SET ANSI_NULLS ON
                                                                                          GO
                                                                                          SET QUOTED_IDENTIFIER ON
                                                                                          GO
                                                                                          -- =============================================
                                                                                          -- Author:		<Ani Kocharyan="">
                                                                                            -- Create date: <09.04.13>
                                                                                              -- Description:	<Counting Local="" TriangleTrajectories="" for="" an="" assembly="" (for="" all="" realizations="")>
                                                                                                -- =============================================
                                                                                                CREATE PROCEDURE [dbo].[CountAllLocalTriangleTrajectories]
                                                                                                @AssID uniqueidentifier
                                                                                                AS
                                                                                                BEGIN
                                                                                                SET NOCOUNT ON;

                                                                                                DECLARE @paramID int;

                                                                                                DECLARE ResCursor CURSOR FOR
                                                                                                SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                                                                OPEN ResCursor;
                                                                                                FETCH FROM ResCursor INTO @paramID;
                                                                                                WHILE @@FETCH_STATUS = 0
                                                                                                BEGIN
                                                                                                EXEC CountLocalTriangleTrajectories @AssID,@paramID;
                                                                                                FETCH NEXT FROM ResCursor INTO @paramID;
                                                                                                END;
                                                                                                CLOSE ResCursor;
                                                                                                DEALLOCATE ResCursor;

                                                                                                DECLARE @CountOfRealizations int;
                                                                                                SELECT @CountOfRealizations = COUNT(*) FROM AssemblyResults WHERE AssemblyID = @AssID;

                                                                                                UPDATE TriangleTrajectoryLocal SET Distribution = Distribution / @CountOfRealizations
                                                                                                WHERE AssemblyID = @AssID;
                                                                                                END

                                                                                                GO
                                                                                                /****** Object:  StoredProcedure [dbo].[CountTriangleTrajectoryAvgSigma]    Script Date: 04/15/2013 10:25:39 ******/
                                                                                                SET ANSI_NULLS ON
                                                                                                GO
                                                                                                SET QUOTED_IDENTIFIER ON
                                                                                                GO
                                                                                                -- =============================================
                                                                                                -- Author:		<Ani Kocharyan="">
                                                                                                  -- Create date: <12.04.13>
                                                                                                    -- Description:	<Counting Average="" and="" Sigma="" for="" an="" assembly="">
                                                                                                      -- =============================================
                                                                                                      CREATE PROCEDURE [dbo].[CountTriangleTrajectoryAvgSigma]
                                                                                                      @AssID uniqueidentifier,
                                                                                                      @StepsToRemove int = 0
                                                                                                      AS
                                                                                                      BEGIN
                                                                                                      SET NOCOUNT ON;

                                                                                                      IF EXISTS(SELECT * FROM TriangleTrajectoryAvgSigma WHERE AssemblyID = @AssID AND StepsToRemove = @StepsToRemove)
                                                                                                      RETURN
                                                                                                      ELSE
                                                                                                      BEGIN
                                                                                                      DECLARE @avg float
                                                                                                      DECLARE @sigma float

                                                                                                      SELECT @avg = AVG(Distribution) FROM TriangleTrajectoryLocal
                                                                                                      WHERE AssemblyID = @AssID AND [Time] >= @StepsToRemove

                                                                                                      SELECT @sigma = AVG((@avg - Distribution) * (@avg - Distribution)) FROM TriangleTrajectoryLocal
                                                                                                      WHERE AssemblyID = @AssID AND [Time] >= @StepsToRemove

                                                                                                      INSERT INTO TriangleTrajectoryAvgSigma(AssemblyID,StepsToRemove,Average,Sigma)
                                                                                                      VALUES(@AssID,@StepsToRemove,@avg,@sigma)
                                                                                                      END
                                                                                                      END

                                                                                                      GO
                                                                                                      /****** Object:  StoredProcedure [dbo].[FillOptimizationTables]    Script Date: 07/09/2013 23:02:26 ******/
                                                                                                      SET ANSI_NULLS ON
                                                                                                      GO
                                                                                                      SET QUOTED_IDENTIFIER ON
                                                                                                      GO
                                                                                                      -- =============================================
                                                                                                      -- Author:		<Ani Kocharyan="">
                                                                                                        -- Create date: <19.05.13>
                                                                                                          -- Description:	<Filling optimization="" tables="" for="" an="" assembly="" (for="" all="" realizations="")>
                                                                                                            -- =============================================
                                                                                                            CREATE PROCEDURE [dbo].[FillOptimizationTables]
                                                                                                            @AssID uniqueidentifier
                                                                                                            AS
                                                                                                            BEGIN
                                                                                                            SET NOCOUNT ON;

                                                                                                            DECLARE @done bit;
                                                                                                            SELECT @done = OptTables FROM Assemblies WHERE AssemblyID = @AssID;
                                                                                                            IF @done = 'False'
                                                                                                            BEGIN
                                                                                                            -- Fill CoefficientsGlobal and CoefficientsLocal tables
                                                                                                            IF EXISTS(SELECT TOP 1 * FROM Coefficients
                                                                                                            WHERE ResultsID in (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID))
                                                                                                            BEGIN
                                                                                                            EXEC CountAllGlobalCoefficients @AssID;
                                                                                                            EXEC CountAllLocalCoefficients @AssID;
                                                                                                            END

                                                                                                            -- Fill VertexDegreeGlobal and VertexDegreeLocal tables
                                                                                                            IF EXISTS(SELECT TOP 1 * FROM VertexDegree
                                                                                                            WHERE ResultsID in (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID))
                                                                                                            BEGIN
                                                                                                            EXEC CountAllGlobalVertexDegrees @AssID;
                                                                                                            EXEC CountAllLocalVertexDegrees @AssID;
                                                                                                            END

                                                                                                            -- Fill ConSubgraphsLocal table
                                                                                                            IF EXISTS(SELECT TOP 1 * FROM ConSubgraphs
                                                                                                            WHERE ResultsID in (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID))
                                                                                                            BEGIN
                                                                                                            EXEC CountAllLocalConSubgraphs @AssID;
                                                                                                            END

                                                                                                            -- Fill VertexDistanceLocal table
                                                                                                            IF EXISTS(SELECT TOP 1 * FROM VertexDistance
                                                                                                            WHERE ResultsID in (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID))
                                                                                                            BEGIN
                                                                                                            EXEC CountAllLocalVertexDistances @AssID;
                                                                                                            END

                                                                                                            -- Fill EigenValuesDistanceLocal table
                                                                                                            IF EXISTS(SELECT TOP 1 * FROM EigenValuesDistance
                                                                                                            WHERE ResultsID in (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID))
                                                                                                            BEGIN
                                                                                                            EXEC CountAllLocalEigenValuesDistances @AssID;
                                                                                                            END

                                                                                                            -- Fill TriangleTrajectoryLocal table
                                                                                                            IF EXISTS(SELECT TOP 1 * FROM TriangleTrajectory
                                                                                                            WHERE ResultsID in (SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID))
                                                                                                            BEGIN
                                                                                                            EXEC CountAllLocalTriangleTrajectories @AssID;
                                                                                                            END

                                                                                                            UPDATE Assemblies SET OptTables = 'True' WHERE AssemblyID = @AssID;
                                                                                                            END
                                                                                                            END

                                                                                                            GO
                                                                                                            /****** Object:  StoredProcedure [dbo].[DeleteAssembly]    Script Date: 05/25/2013 15:36:43 ******/
                                                                                                            SET ANSI_NULLS ON
                                                                                                            GO
                                                                                                            SET QUOTED_IDENTIFIER ON
                                                                                                            GO
                                                                                                            -- =============================================
                                                                                                            -- Author:		<Ani Kocharyan="">
                                                                                                              -- Create date: <25.05.13>
                                                                                                                -- Description:	<Deleting an="" assembly="">
-- =============================================
CREATE PROCEDURE [dbo].[DeleteAssembly]
	@AssID uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM AnalyzeResults WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);

	DELETE FROM Coefficients WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);
	DELETE FROM CoefficientsGlobal WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);
	DELETE FROM CoefficientsLocal WHERE AssemblyID = @AssID;

	DELETE FROM ConSubgraphs WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);
	DELETE FROM ConSubgraphsLocal WHERE AssemblyID = @AssID;

	DELETE FROM Cycles WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);

	DELETE FROM EigenValues WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);

	DELETE FROM EigenValuesDistance WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);
	DELETE FROM EigenValuesDistanceLocal WHERE AssemblyID = @AssID;

	DELETE FROM FullSubgraphs WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);

	DELETE FROM Motifs WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);

	DELETE FROM Triangles WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);

	DELETE FROM TriangleTrajectory WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);
	DELETE FROM TriangleTrajectoryAvgSigma WHERE AssemblyID = @AssID;
	DELETE FROM TriangleTrajectoryLocal WHERE AssemblyID = @AssID;
	
	DELETE FROM VertexDegree WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);
	DELETE FROM VertexDegreeGlobal WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);
	DELETE FROM VertexDegreeLocal WHERE AssemblyID = @AssID;
	
	DELETE FROM VertexDistance WHERE ResultsID IN 
		(SELECT ResultsID FROM AssemblyResults WHERE AssemblyID = @AssID);
	DELETE FROM VertexDistanceLocal WHERE AssemblyID = @AssID;

	DELETE FROM AssemblyResults WHERE AssemblyID = @AssID;
	DELETE FROM GenerationParamValues WHERE AssemblyID = @AssID;
	DELETE FROM AnalyzeOptionParamValues WHERE AssemblyID = @AssID;
	
	DELETE FROM Assemblies WHERE AssemblyID = @AssID;
END