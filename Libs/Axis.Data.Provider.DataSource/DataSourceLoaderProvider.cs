using Axis.Data.Abstraction;
using Microsoft.Extensions.Configuration;

namespace Axis.Data.Provider.DataSource;

public class DataSourceLoaderProvider : ConfigurationProvider, IConfigurationSource {

  private readonly IConfigurationBuilder _builder;
  private readonly DataSourceLoaderOptions _options;

  public DataSourceLoaderProvider(IConfigurationBuilder builder, DataSourceLoaderOptions options) {
    _builder = builder;
    _options = options;
  }

  public override void Load() {
    // check path is not empty
    if (string.IsNullOrEmpty(_options.Path)) {
      return;
    }
    // check folder is exists
    DirectoryInfo dir = new(_options.Path);
    if (dir.Exists == false) {
      return;
    }
    Dictionary<string, string?> connection_strings = new();
    // get database connection file
    foreach (var file in dir.GetFiles("*.db")) {
      // resolve file
      var options = resolve(file);
      if (options != null &&
        string.IsNullOrEmpty(options.ConnectionName) == false &&
        string.IsNullOrEmpty(options.ConnectionString) == false) {
        string key = $"ConnectionStrings:{options.ConnectionName}";
        string value = options.ConnectionString;
        if (connection_strings.ContainsKey(key) == false) {
          connection_strings.Add(key, value);
        }
        else {
          connection_strings[key] = value;
        }
      }
    }
    // set to provider data
    Data = connection_strings;
  }

  private DatabaseOptions? resolve(FileInfo file) {
    string text = File.OpenRead(file.FullName).GzDecompress();
    DatabaseOptions? options = DatabaseOptions.Load(text);
    return options;
  }

  public IConfigurationProvider Build(IConfigurationBuilder builder) {
    return new DataSourceLoaderProvider(builder, _options);
  }

}