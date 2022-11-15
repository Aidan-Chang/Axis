using Microsoft.Extensions.Configuration;

namespace Axis.Data.DatabaseConnection;

public static class DatabaseConnectionLoader {

  public static IConfigurationBuilder AddDbConnections(this IConfigurationBuilder builder, Action<DatabaseConnectionLoaderOptions> action) {
    var options = new DatabaseConnectionLoaderOptions();
    action?.Invoke(options);
    builder.Add(new DatabaseConnectionLoaderProvider(builder, options));
    return builder;
  }

}