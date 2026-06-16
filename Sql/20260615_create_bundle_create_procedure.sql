CREATE OR ALTER PROCEDURE dbo.sp_fn_bundle_i_bundle
    @bundleCode varchar(100),
    @idBundleType int,
    @headquarter varchar(100),
    @camera varchar(100),
    @steelDiameter varchar(100),
    @itemCount int,
    @countedAt datetime2(7),
    @videoPath nvarchar(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Bundle
    (
        idSteelType,
        idCamera,
        isAlert,
        itemCount,
        videoPath,
        code,
        idBundleType,
        createdAt,
        isActive
    )
    OUTPUT INSERTED.idBundle
    VALUES
    (
        0,
        0,
        0,
        @itemCount,
        @videoPath,
        @bundleCode,
        @idBundleType,
        @countedAt,
        1
    );
END;
GO
