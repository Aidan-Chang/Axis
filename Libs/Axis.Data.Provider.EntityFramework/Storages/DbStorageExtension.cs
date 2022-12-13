using Microsoft.EntityFrameworkCore;

namespace Axis.Data.Provider.EntityFramework.Storages;

public static class DbStorageExtension {

  public static DbContextOptionsBuilder UseStorage(
    this DbContextOptionsBuilder optionsBuilder,
    string databaseType,
    string connectionString) {
    return databaseType switch {
      "PostgreSql" => optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("migrations", "app")),
      "MsSql" => optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsHistoryTable("migrations", "app")),
      _ => optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsHistoryTable("migrations", "app")),
    };
  }

}
