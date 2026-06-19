CREATE OR ALTER PROCEDURE dbo.sp_fn_bundle_i_bundle
    @bundleCode varchar(100),
    @idBundleType int,
    @headquarter varchar(100),
    @camera varchar(100),
    @steelDiameter varchar(100),
    @itemCount int,
    @countStartedAt datetime2(7),
    @countFinishedAt datetime2(7),
    @countTime varchar(8),
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
        countStartedAt,
        countFinishedAt,
        countTime,
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
        @countStartedAt,
        @countFinishedAt,
        @countTime,
        @videoPath,
        @bundleCode,
        @idBundleType,
        SYSUTCDATETIME(),
        1
    );
END;
GO
