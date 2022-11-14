using Microsoft.Extensions.Configuration;

namespace Axis.Data.Abstraction;

public class DatabaseConnectionProvider : ConfigurationProvider, IConfigurationSource {

  private readonly IConfigurationBuilder _builder;
  private readonly DatabaseConnectionOptions _options;

  public DatabaseConnectionProvider(IConfigurationBuilder builder, DatabaseConnectionOptions options) {
    _builder = builder;
    _options = options;
  }

  public override void Load() {
    Data = LoadData();
  }

  private IDictionary<string, string?> LoadData() {
    Dictionary<string, string?> connection_strings = new();
    return connection_strings;
  }

  public IConfigurationProvider Build(IConfigurationBuilder builder) {
    return new DatabaseConnectionProvider(builder, _options);
  }

}
