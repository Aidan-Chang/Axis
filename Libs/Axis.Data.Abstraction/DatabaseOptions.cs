namespace Axis.Data.Abstraction;

public class DatabaseOptions {

  public string? ProviderName { get; set; }

  public string? Server { get; set; }

  public int? Port { get; set; }

  public string? DatabaseName { get; set; }

  public string? ConnectionName { get; set; }

  public string? ConnectionString { get; set; }

  public TimeSpan CommandTimeout { get; set; } = new TimeSpan(0, 0, 30);

}
