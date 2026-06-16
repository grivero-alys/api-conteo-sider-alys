IF OBJECT_ID('dbo.UQ_Bundle_BarCode', 'UQ') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Bundle DROP CONSTRAINT UQ_Bundle_BarCode;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Bundle_barCode'
      AND object_id = OBJECT_ID('dbo.Bundle')
)
BEGIN
    DROP INDEX UX_Bundle_barCode ON dbo.Bundle;
END;
GO
