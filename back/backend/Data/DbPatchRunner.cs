using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Data;

/// <summary>
/// Deterministic, ordered SQL patch runner with its own history table.
/// Replaces both the ad-hoc RunSqlMigrationAsync and EnsureCreated approaches:
/// fresh databases receive every patch; legacy databases (base tables already
/// present) are baselined automatically and only receive new patches.
/// </summary>
public static class DbPatchRunner
{
    private const string HistoryTable = "_schema_patches";

    public static async Task RunAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector;");

        var conn = (NpgsqlConnection)db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();

        await EnsureHistoryTableAsync(conn);

        var applied = await GetAppliedPatchesAsync(conn);
        var usersExists = await TableExistsAsync(conn, "users");

        foreach (var (name, sql) in LoadEmbeddedPatches())
        {
            if (applied.Contains(name))
                continue;

            // Baseline: if the base patch's objects already exist from a legacy
            // deployment, record it without executing (idempotent adoption).
            if (name == "000_base" && usersExists)
            {
                await RecordPatchAsync(conn, name);
                Console.WriteLine($"[PATCH] {name}: baseline recorded (legacy schema adopted).");
                continue;
            }

            try
            {
                await using var tx = await conn.BeginTransactionAsync();
                var cmd = conn.CreateCommand();
                cmd.Transaction = (NpgsqlTransaction)tx;
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync();
                await RecordPatchAsync(conn, name, tx);
                await (Task)tx.CommitAsync();
                Console.WriteLine($"[PATCH] {name}: applied.");
            }
            catch (PostgresException ex) when (ex.SqlState is "42710" or "42P07" or "42701")
            {
                // Object already exists — adopt and continue.
                await RecordPatchAsync(conn, name);
                Console.WriteLine($"[PATCH] {name}: partially existing, recorded ({ex.Message.Split('\n')[0]}).");
            }
        }
    }

    private static (string Name, string Sql)[] LoadEmbeddedPatches()
    {
        var asm = Assembly.GetExecutingAssembly();
        return asm.GetManifestResourceNames()
            .Where(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n, StringComparer.Ordinal)
            .Select(n =>
            {
                using var stream = asm.GetManifestResourceStream(n)!;
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();
                var name = n.Substring(n.LastIndexOf('.') + 1);
                return (Name: name, Sql: content);
            })
            .ToArray();
    }

    private static async Task EnsureHistoryTableAsync(NpgsqlConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
            CREATE TABLE IF NOT EXISTS {HistoryTable} (
                name varchar(128) PRIMARY KEY,
                applied_at timestamptz NOT NULL DEFAULT now()
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<System.Collections.Generic.HashSet<string>> GetAppliedPatchesAsync(
        NpgsqlConnection conn)
    {
        var result = new System.Collections.Generic.HashSet<string>();
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT name FROM {HistoryTable}";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(reader.GetString(0));
        return result;
    }

    private static async Task RecordPatchAsync(NpgsqlConnection conn, string name,
        NpgsqlTransaction? tx = null)
    {
        var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = $"INSERT INTO {HistoryTable}(name) VALUES (@name) ON CONFLICT DO NOTHING";
        var p = cmd.CreateParameter();
        p.ParameterName = "name";
        p.Value = name;
        cmd.Parameters.Add(p);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<bool> TableExistsAsync(NpgsqlConnection conn, string table)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = @t)";
        var p = cmd.CreateParameter();
        p.ParameterName = "t";
        p.Value = table;
        cmd.Parameters.Add(p);
        var o = await cmd.ExecuteScalarAsync();
        return o is bool b && b;
    }
}
