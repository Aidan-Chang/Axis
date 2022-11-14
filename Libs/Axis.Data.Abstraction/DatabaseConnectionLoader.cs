using Microsoft.Extensions.Configuration;

namespace Axis.Data.Abstraction;

public static class DatabaseConnectionLoader {
  public static IConfigurationBuilder AddDbConnections(this IConfigurationBuilder builder, Action<DatabaseConnectionOptions> action) {
    var options = new DatabaseConnectionOptions();
    action?.Invoke(options);
    builder.Add(new DatabaseConnectionProvider(builder, options));
    return builder;
  }
}