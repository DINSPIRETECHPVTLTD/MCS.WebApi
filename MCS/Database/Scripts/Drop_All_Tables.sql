-- Drops all user tables, foreign keys, and EF Core migration history in the current database.
-- Use with caution: THIS WILL DELETE ALL DATA.
-- Recommended usage:
-- 1. Backup your database.
-- 2. Connect to the target database in SSMS or sqlcmd.
-- 3. Run this script.

SET NOCOUNT ON;

PRINT 'Dropping all foreign key constraints...';

DECLARE @sql NVARCHAR(MAX) = N'';

;WITH FK_Constraints AS (
    SELECT
        fk.name AS FK_Name,
        sch.name AS SchemaName,
        tab.name AS TableName
    FROM sys.foreign_keys fk
    INNER JOIN sys.tables tab ON fk.parent_object_id = tab.object_id
    INNER JOIN sys.schemas sch ON tab.schema_id = sch.schema_id
    WHERE fk.is_ms_shipped = 0
)
SELECT @sql = @sql + N'
ALTER TABLE [' + SchemaName + '].[' + TableName + '] DROP CONSTRAINT [' + FK_Name + '];'
FROM FK_Constraints;

IF (@sql <> N'')
BEGIN
    PRINT 'Executing foreign key drop statements...';
    EXEC sp_executesql @sql;
END
ELSE
BEGIN
    PRINT 'No foreign keys found to drop.';
END

PRINT 'Dropping all user tables...';

SET @sql = N'';

;WITH UserTables AS (
    SELECT
        sch.name AS SchemaName,
        tab.name AS TableName
    FROM sys.tables tab
    INNER JOIN sys.schemas sch ON tab.schema_id = sch.schema_id
    WHERE tab.is_ms_shipped = 0
      AND tab.name <> 'sysdiagrams'
)
SELECT @sql = @sql + N'
DROP TABLE [' + SchemaName + '].[' + TableName + '];'
FROM UserTables;

IF (@sql <> N'')
BEGIN
    PRINT 'Executing table drop statements...';
    EXEC sp_executesql @sql;
END
ELSE
BEGIN
    PRINT 'No user tables found to drop.';
END

PRINT 'Dropping EF Core migrations history table if it exists...';

IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[__EFMigrationsHistory];
    PRINT '__EFMigrationsHistory table dropped.';
END
ELSE
BEGIN
    PRINT '__EFMigrationsHistory table not found.';
END

PRINT 'Drop all tables script completed.';

