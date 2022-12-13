using Hangfire;
using Hangfire.PostgreSql;

namespace Axis.Web.Extension.Worker.Storages;

public static class HangfireStorageExtension {
  public static IGlobalConfiguration<JobStorage> UseHangfireStorage(this IGlobalConfiguration configuration, string databaseType, string connectionString) {
    return databaseType switch {
      "PostgreSql" => configuration.UsePostgreSqlStorage(connectionString),
      "MsSql" => configuration.UseSqlServerStorage(connectionString),
      _ => configuration.UseSqlServerStorage(connectionString),
    };
  }

}