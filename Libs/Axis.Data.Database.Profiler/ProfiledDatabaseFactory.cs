using Axis.Data.Abstraction;
using Npgsql;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using System.Data.Common;
namespace Axis.Data.Database.Profiler;

public static class ProfiledDatabaseFactory {

  public static DbConnection CreateProfiledDatabase(this DatabaseOptions options) {
    DbConnection connection = options.ProviderName switch {
      "PostgreSql" => new NpgsqlConnection(options.ConnectionString),
      _ => throw new NotSupportedException($"Database Provier Name - {options.ProviderName} - is not support")
    };
    return new ProfiledDbConnection(connection, MiniProfiler.Current);
  }

}
