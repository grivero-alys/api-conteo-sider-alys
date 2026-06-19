using api_conteo_sider_alys.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace api_conteo_sider_alys.Repositories.Bundle;

public sealed class BundleRepository : IBundleRepository
{
    private readonly string? _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

    public async Task<int> CreateAsync(
        BundleCreationInput input,
        string bundleCode,
        string? videoPath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("SqlConnectionString is required.");
        }

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.sp_fn_bundle_i_bundle";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@bundleCode", SqlDbType.VarChar, 100).Value = bundleCode;
        command.Parameters.Add("@idBundleType", SqlDbType.Int).Value = BundleTypes.GetId(input.BundleType);
        command.Parameters.Add("@headquarter", SqlDbType.VarChar, 100).Value = input.Headquarter;
        command.Parameters.Add("@camera", SqlDbType.VarChar, 100).Value = input.Camera;
        command.Parameters.Add("@steelDiameter", SqlDbType.VarChar, 100).Value = input.SteelDiameter;
        command.Parameters.Add("@itemCount", SqlDbType.Int).Value = input.ItemCount;
        command.Parameters.Add("@countStartedAt", SqlDbType.DateTime2).Value = input.CountStartedAt.UtcDateTime;
        command.Parameters.Add("@countFinishedAt", SqlDbType.DateTime2).Value = input.CountFinishedAt.UtcDateTime;
        command.Parameters.Add("@countTime", SqlDbType.VarChar, 8).Value = input.CountTime;
        command.Parameters.Add("@videoPath", SqlDbType.NVarChar, 500).Value = (object?)videoPath ?? DBNull.Value;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }
}
