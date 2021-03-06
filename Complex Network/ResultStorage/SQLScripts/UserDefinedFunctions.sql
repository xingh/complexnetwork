/****** Object:  UserDefinedFunction [dbo].[RealizationsCount]    Script Date: 05/29/2013 15:27:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Ani Kocharyan>
-- Create date: <28.05.13>
-- Description:	<Returns the number of realizations for an assembly>
-- =============================================
CREATE FUNCTION [dbo].[RealizationsCount]
(
	@AssID uniqueidentifier
)
RETURNS int
AS
BEGIN
	DECLARE @result int
	SELECT @result = COUNT(*) FROM AssemblyResults WHERE AssemblyID = @AssID

	RETURN @result
END
