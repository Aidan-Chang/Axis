namespace Axis.Plugin.Abstractin;

public class PluginOptions {

  public string Path { get; set; } = string.Empty;

  public string Pattern { get; set; } = string.Empty;

  public Type[]? Types { get; set; }

  public bool PreferSharedTypes { get; set; } = true;

  public bool IsUnloadable { get; set; } = false;

  public bool IsLazyLoaded { get; set; } = false;

  public bool EnableHotReload { get; set; } = false;

}