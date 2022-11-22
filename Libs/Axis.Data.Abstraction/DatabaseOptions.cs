using System.Text.Json;

namespace Axis.Data.Abstraction;

public class DatabaseOptions {

  public string? ProviderName { get; set; }

  public string? Server { get; set; }

  public int? Port { get; set; }

  public string? DatabaseName { get; set; }

  public string? ConnectionName { get; set; }

  public string? ConnectionString { get; set; }

  public TimeSpan CommandTimeout { get; set; } = new TimeSpan(0, 0, 30);

  public override string ToString() {
    DatabaseOptions? clone = MemberwiseClone() as DatabaseOptions;
    if (clone != null) {
      string text = DataUtility.AesEncrypt(clone.ConnectionString, clone.Server ?? string.Empty, clone.DatabaseName ?? string.Empty);
      clone.ConnectionString = text;
      return JsonSerializer.Serialize(clone);
    }
    return string.Empty;
  }

  public static DatabaseOptions? Load(string value) {
    DatabaseOptions? options = JsonSerializer.Deserialize<DatabaseOptions>(value ?? string.Empty);
    if (options != null) {
      options.ConnectionString = DataUtility.AesDecrypt(options.ConnectionString ?? string.Empty, options.Server ?? string.Empty, options.DatabaseName ?? string.Empty);
      return options;
    }
    return null;
  }

}
