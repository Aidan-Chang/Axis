using Axis.Plugin.Abstractin;
using Axis.Plugin.Storage;

namespace Axis.Plugin;

public class PluginCollection {

  private readonly Dictionary<string, PluginEntry> _loader = new();

  public string[] Names => _loader.Keys.ToArray();

  public PluginEntry? this[string key] {
    get => _loader.ContainsKey(key) ? _loader[key] : null;
    set {
      if (value != null) {
        _loader[key] = value;
      }
    }
  }

  public string BasePath { get; set; } = string.Empty;

  public IPluginLoaderStorage? Storage { get; set; }

  public void Load() {
    if (Storage == null) return;
    Dictionary<string, PluginEntry>? data = Storage.Load();
    if (data != null) {
      foreach (var key in data.Keys) {
        this[key] = data[key];
      }
    }
  }

  public void Save() {
    if (Storage == null) return;
    Storage.Save(_loader);
  }

}
