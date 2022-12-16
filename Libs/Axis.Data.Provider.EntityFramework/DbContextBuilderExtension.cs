using Axis.Data.Abstraction;
using Axis.Data.Database.NamingConvention;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Axis.Data.Provider.EntityFramework;

public static class DbContextBuilderExtension {

  public static IServiceCollection AddDbContextBuilder(
  this IServiceCollection services,
  Action<DatabaseOptions> options) {
    DatabaseOptions opt = new DatabaseOptions();
    options.Invoke(opt);
    // Add database context builder
    services.AddSingleton(provider =>
      new Action<DbContextOptionsBuilder>(options => {
        options
          .EnableSensitiveDataLogging()
          .UseLoggerFactory(provider.GetService<ILoggerFactory>())
          .UseStorage(
            opt.ProviderName ?? "",
            opt.ConnectionString ?? "")
          .UseNamingConvention(name: opt.NamingConvention);
      }));
    return services;
  }

}
