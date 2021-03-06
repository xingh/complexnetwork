/****** Object:  Table [dbo].[CoefficientsGlobal]    Script Date: 04/12/2013 10:10:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CoefficientsGlobal](
[ResultsID] [int] NOT NULL,
[AvgCoefficient] [float] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[CoefficientsGlobal]  WITH CHECK ADD  CONSTRAINT [FK_CoefficientsGlobal_AssemblyResults] FOREIGN KEY([ResultsID])
REFERENCES [dbo].[AssemblyResults] ([ResultsID])
GO
ALTER TABLE [dbo].[CoefficientsGlobal] CHECK CONSTRAINT [FK_CoefficientsGlobal_AssemblyResults]

/****** Object:  Table [dbo].[CoefficientsLocal]    Script Date: 04/12/2013 10:10:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CoefficientsLocal](
[AssemblyID] [uniqueidentifier] NOT NULL,
[Coefficient] [float] NOT NULL,
[Distribution] [float] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[CoefficientsLocal]  WITH CHECK ADD  CONSTRAINT [FK_CoefficientsLocal_Assemblies] FOREIGN KEY([AssemblyID])
REFERENCES [dbo].[Assemblies] ([AssemblyID])
GO
ALTER TABLE [dbo].[CoefficientsLocal] CHECK CONSTRAINT [FK_CoefficientsLocal_Assemblies]

/****** Object:  Table [dbo].[VertexDegreeGlobal]    Script Date: 04/12/2013 10:11:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VertexDegreeGlobal](
[ResultsID] [int] NOT NULL,
[AvgDegree] [float] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[VertexDegreeGlobal]  WITH CHECK ADD  CONSTRAINT [FK_VertexDegreeGlobal_AssemblyResults] FOREIGN KEY([ResultsID])
REFERENCES [dbo].[AssemblyResults] ([ResultsID])
GO
ALTER TABLE [dbo].[VertexDegreeGlobal] CHECK CONSTRAINT [FK_VertexDegreeGlobal_AssemblyResults]

/****** Object:  Table [dbo].[VertexDegreeLocal]    Script Date: 04/12/2013 10:12:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VertexDegreeLocal](
[AssemblyID] [uniqueidentifier] NOT NULL,
[Degree] [int] NOT NULL,
[Distribution] [float] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[VertexDegreeLocal]  WITH CHECK ADD  CONSTRAINT [FK_VertexDegreeLocal_Assemblies] FOREIGN KEY([AssemblyID])
REFERENCES [dbo].[Assemblies] ([AssemblyID])
GO
ALTER TABLE [dbo].[VertexDegreeLocal] CHECK CONSTRAINT [FK_VertexDegreeLocal_Assemblies]

/****** Object:  Table [dbo].[ConSubgraphsLocal]    Script Date: 04/12/2013 10:10:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConSubgraphsLocal](
[AssemblyID] [uniqueidentifier] NOT NULL,
[VX] [int] NOT NULL,
[Distribution] [float] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[ConSubgraphsLocal]  WITH CHECK ADD  CONSTRAINT [FK_ConSubgraphsLocal_Assemblies] FOREIGN KEY([AssemblyID])
REFERENCES [dbo].[Assemblies] ([AssemblyID])
GO
ALTER TABLE [dbo].[ConSubgraphsLocal] CHECK CONSTRAINT [FK_ConSubgraphsLocal_Assemblies]

/****** Object:  Table [dbo].[EigenValuesDistanceLocal]    Script Date: 04/12/2013 10:11:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EigenValuesDistanceLocal](
[AssemblyID] [uniqueidentifier] NOT NULL,
[Distance] [float] NOT NULL,
[Distribution] [float] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[EigenValuesDistanceLocal]  WITH CHECK ADD  CONSTRAINT [FK_EigenValuesDistanceLocal_Assemblies] FOREIGN KEY([AssemblyID])
REFERENCES [dbo].[Assemblies] ([AssemblyID])
GO
ALTER TABLE [dbo].[EigenValuesDistanceLocal] CHECK CONSTRAINT [FK_EigenValuesDistanceLocal_Assemblies]

/****** Object:  Table [dbo].[VertexDistanceLocal]    Script Date: 04/12/2013 10:12:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VertexDistanceLocal](
[AssemblyID] [uniqueidentifier] NOT NULL,
[Distance] [int] NOT NULL,
[Distribution] [float] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[VertexDistanceLocal]  WITH CHECK ADD  CONSTRAINT [FK_VertexDistanceLocal_Assemblies] FOREIGN KEY([AssemblyID])
REFERENCES [dbo].[Assemblies] ([AssemblyID])
GO
ALTER TABLE [dbo].[VertexDistanceLocal] CHECK CONSTRAINT [FK_VertexDistanceLocal_Assemblies]

/****** Object:  Table [dbo].[TriangleTrajectoryLocal]    Script Date: 04/12/2013 10:11:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TriangleTrajectoryLocal](
[AssemblyID] [uniqueidentifier] NOT NULL,
[Time] [int] NOT NULL,
[Distribution] [float] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[TriangleTrajectoryLocal]  WITH CHECK ADD  CONSTRAINT [FK_TriangleTrajectoryLocal_Assemblies] FOREIGN KEY([AssemblyID])
REFERENCES [dbo].[Assemblies] ([AssemblyID])
GO
ALTER TABLE [dbo].[TriangleTrajectoryLocal] CHECK CONSTRAINT [FK_TriangleTrajectoryLocal_Assemblies]

/****** Object:  Table [dbo].[TriangleTrajectoryAvgSigma]    Script Date: 04/15/2013 10:19:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TriangleTrajectoryAvgSigma](
[AssemblyID] [uniqueidentifier] NOT NULL,
[StepsToRemove] [int] NOT NULL,
[Average] [float] NOT NULL,
[Sigma] [float] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[TriangleTrajectoryAvgSigma]  WITH CHECK ADD  CONSTRAINT [FK_TriangleTrajectoryAvgSigma_Assemblies] FOREIGN KEY([AssemblyID])
REFERENCES [dbo].[Assemblies] ([AssemblyID])
GO
ALTER TABLE [dbo].[TriangleTrajectoryAvgSigma] CHECK CONSTRAINT [FK_TriangleTrajectoryAvgSigma_Assemblies]