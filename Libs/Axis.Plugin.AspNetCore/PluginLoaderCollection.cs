namespace Axis.Plugin.AspNetCore {

  public class PluginLoaderCollection {

    private readonly Dictionary<string, PluginLoader> _loader = new();

    public string[] Names => _loader.Keys.ToArray();

    public PluginLoader? this[string key] {
      get => _loader.ContainsKey(key) ? _loader[key] : null;
      set {
        if (value != null) {
          _loader[key] = value;
        }
      }
    }

  }

}
