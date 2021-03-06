﻿ALTER PROCEDURE KK_PARAM
	 @CODE as int
	,@PARAM as nvarchar(150)
	,@DIFF1 as nvarchar(150) = null
	,@DIFF2 as nvarchar(150) = null
	,@DIFF3 as nvarchar(150) = null
	,@BPLID as int = 0
WITH ENCRYPTION AS

DECLARE @sql as nvarchar(MAX);

SET @sql = N'
SELECT	 U_VALUE as Value
		,U_DIFF1 as Diff1
		,U_DIFF2 as Diff2
		,U_DIFF3 as Diff3
FROM	 [@TS_PARAM1]
WHERE	 Code = ' + CAST(@CODE as nvarchar(4)) + '
	AND	 U_PARAM = ''' + @PARAM + '''
	AND	 (U_DUEDATE <= GETDATE()
		OR U_DUEDATE IS NULL)
	AND	(U_BPLID = ' + CAST(@BPLID as nvarchar(4)) + ' 
		OR U_BPLID IS NULL)
'

IF @DIFF1 IS NOT NULL AND @DIFF1 <> '' SET @sql = @sql + N'	AND	 U_DIFF1 = ''' + @DIFF1 + '''' + CHAR(13)
IF @DIFF2 IS NOT NULL AND @DIFF2 <> '' SET @sql = @sql + N'	AND	 U_DIFF2 = ''' + @DIFF2 + '''' + CHAR(13)
IF @DIFF3 IS NOT NULL AND @DIFF2 <> '' SET @sql = @sql + N'	AND	 U_DIFF3 = ''' + @DIFF3 + '''' + CHAR(13)

SET @sql = @sql + N'ORDER BY U_ORDER ASC' 
	
EXEC(@sql)

go

KK_PARAM 2002, 'Printer' --, '11', '222', 1123