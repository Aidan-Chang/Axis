using System.Text.Json;

namespace Axis.Plugin.AspNetCore.Storage;

public class PluginLoaderFileStorage : IPluginLoaderStorage {

  public string Path { get; private set; }

  public PluginLoaderFileStorage(string path) {
    if (string.IsNullOrEmpty(path) == true) {
      throw new NullReferenceException($"Path is empty");
    }
    Path = System.IO.Path.Combine(path, "package.json");
  }

  public void Save(Dictionary<string, PluginInfo> collection) {
    string text = JsonSerializer.Serialize(collection, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(Path, text);
  }

  public Dictionary<string, PluginInfo> Load() {
    if (File.Exists(Path) == false) {
      FileStream stream = File.Create(Path);
      stream.Close();
    }
    string text = File.ReadAllText(Path);
    if (string.IsNullOrEmpty(text) == true) {
      return new Dictionary<string, PluginInfo>();
    }
    return JsonSerializer.Deserialize<Dictionary<string, PluginInfo>>(text) ?? new Dictionary<string, PluginInfo>();
  }

}
