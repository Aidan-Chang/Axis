using Microsoft.EntityFrameworkCore;

namespace Axis.Data.Provider.EntityFramework;

public static class DbStorageExtension {

  public static DbContextOptionsBuilder UseStorage(
    this DbContextOptionsBuilder builder,
    string providerName,
    string connectionString) {
    return providerName switch {
      "PostgreSql" => builder.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("migrations", "app")),
      "MsSql" => builder.UseSqlServer(connectionString, x => x.MigrationsHistoryTable("migrations", "app")),
      _ => builder.UseSqlServer(connectionString, x => x.MigrationsHistoryTable("migrations", "app")),
    };
  }

}
