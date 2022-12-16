using Axis.Data.Abstraction;
using Axis.Data.SqlBuilder.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Data.Provider.SqlBuilder;

public static class SqlBuilderExtension {

  public static IServiceCollection AddSqlBuilder(
  this IServiceCollection services,
  Action<DatabaseOptions> options) {
    DatabaseOptions opt = new DatabaseOptions();
    options.Invoke(opt);
    // Add query factory
    services.AddScoped(provider => new QueryFactory());
    services.AddSingleton<Func<QueryFactory>>(
      provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<QueryFactory>());
    return services;
  }

}
