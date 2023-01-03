using Axis.Data.Abstraction;
using Axis.Data.Database.Profiler;
using Axis.Data.SqlBuilder.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Data.Provider.SqlBuilder;

public static class SqlBuilderExtension {

  public static IServiceCollection AddSqlBuilder(
    this IServiceCollection services,
    Action<DatabaseOptions> action) {
    DatabaseOptions options = new DatabaseOptions();
    action.Invoke(options);
    // Add query factory
    services.AddScoped(
      provider => new QueryFactory(options.CreateConnection()));
    services.AddSingleton<Func<QueryFactory>>(
      provider =>
        () => provider.CreateScope().ServiceProvider.GetRequiredService<QueryFactory>());
    return services;
  }

}
