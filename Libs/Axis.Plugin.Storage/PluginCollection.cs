using Axis.Plugin.Abstractin;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Axis.Plugin.Storage;

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
  public string Pattern { get; set; } = string.Empty;

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

  public void FindAndUpdateFromPluginAssembly(Func<PluginEntry, IPluginLoader> callback) {
    DirectoryInfo root = new(BasePath);
    foreach (var dir in root.GetDirectories(Pattern)) {
      foreach (var file in dir.GetFiles($"{dir.Name}.dll")) {
        // same name with dll file name and directory name
        if (dir.Name != file.Name.Replace(file.Extension, string.Empty)) {
          continue;
        }
        PluginEntry entry = this[dir.Name] ?? new PluginEntry() {
          Name = dir.Name,
          Enabled = true,
        };
        entry.Path = file.FullName;
        entry.Version = FileVersionInfo.GetVersionInfo(file.FullName)?.FileVersion ?? "";
        if (entry.Enabled == true) {
          if (callback != null) {
            entry.Loader = callback(entry);
          }
          // add loader to list
          this[dir.Name] = entry;
        }
      }
    }
  }

}
