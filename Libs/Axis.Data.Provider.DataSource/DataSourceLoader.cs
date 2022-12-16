using Microsoft.Extensions.Configuration;

namespace Axis.Data.Provider.DataSource;

public static class DataSourceLoader {

  public static IConfigurationBuilder AddDataSource(this IConfigurationBuilder builder, Action<DataSourceLoaderOptions> action) {
    var options = new DataSourceLoaderOptions();
    action?.Invoke(options);
    builder.Add(new DataSourceLoaderProvider(builder, options));
    return builder;
  }

}