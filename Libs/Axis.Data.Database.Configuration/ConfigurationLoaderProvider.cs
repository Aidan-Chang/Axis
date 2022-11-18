using Axis.Data.Abstraction;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Axis.Data.Database.Configuration;

public class DatabaseConnectionLoaderProvider : ConfigurationProvider, IConfigurationSource {

    private readonly IConfigurationBuilder _builder;
    private readonly DatabaseConnectionLoaderOptions _options;

    public DatabaseConnectionLoaderProvider(IConfigurationBuilder builder, DatabaseConnectionLoaderOptions options) {
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
                string value = DataUtility.AesDecrypt(options.ConnectionString, options.ConnectionName, options.DatabaseName ?? string.Empty);
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
        string text = DataUtility.GzDecompress(file);
        DatabaseOptions? options = JsonSerializer.Deserialize<DatabaseOptions>(text);
        return options;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) {
        return new DatabaseConnectionLoaderProvider(builder, _options);
    }

}
