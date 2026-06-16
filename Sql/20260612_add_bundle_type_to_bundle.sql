IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Bundle_CountType'
      AND parent_object_id = OBJECT_ID('dbo.Bundle')
)
BEGIN
    ALTER TABLE dbo.Bundle DROP CONSTRAINT FK_Bundle_CountType;
END;
GO

IF OBJECT_ID('dbo.DF_Bundle_idCountType', 'D') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Bundle DROP CONSTRAINT DF_Bundle_idCountType;
END;
GO

IF COL_LENGTH('dbo.Bundle', 'idCountType') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Bundle DROP COLUMN idCountType;
END;
GO

IF OBJECT_ID('dbo.CountType', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.CountType;
END;
GO

IF OBJECT_ID('dbo.BundleType', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BundleType
    (
        idBundleType int NOT NULL CONSTRAINT PK_BundleType PRIMARY KEY,
        name varchar(20) NOT NULL CONSTRAINT UQ_BundleType_name UNIQUE,
        createdAt datetime NOT NULL CONSTRAINT DF_BundleType_createdAt DEFAULT (GETDATE()),
        isActive bit NOT NULL CONSTRAINT DF_BundleType_isActive DEFAULT (1)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.BundleType WHERE idBundleType = 1)
BEGIN
    INSERT INTO dbo.BundleType (idBundleType, name) VALUES (1, 'INDIVIDUAL');
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.BundleType WHERE idBundleType = 2)
BEGIN
    INSERT INTO dbo.BundleType (idBundleType, name) VALUES (2, 'AGRUPADO');
END;
GO

IF COL_LENGTH('dbo.Bundle', 'idBundleType') IS NULL
BEGIN
    ALTER TABLE dbo.Bundle
        ADD idBundleType int NOT NULL
            CONSTRAINT DF_Bundle_idBundleType DEFAULT (1);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Bundle_BundleType'
      AND parent_object_id = OBJECT_ID('dbo.Bundle')
)
BEGIN
    ALTER TABLE dbo.Bundle
        ADD CONSTRAINT FK_Bundle_BundleType
            FOREIGN KEY (idBundleType) REFERENCES dbo.BundleType(idBundleType);
END;
GO
