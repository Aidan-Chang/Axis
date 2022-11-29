using System.Text.Json;

namespace Axis.Plugin.AspNetCore;

internal class PluginLoaderStorage {

  public string Path { get; private set; }

  public List<PluginLoaderState> States = new();

  public PluginLoaderStorage(string path) {
    if (string.IsNullOrEmpty(Path) == true) {
      throw new NullReferenceException($"Path is empty");
    }
    Path = path;
    if (File.Exists(Path) == false) {
      using (var stream = File.Create(Path))
      using (var writer = new StreamWriter(stream)) {
        // write default values
        string text = JsonSerializer.Serialize(States);
        writer.WriteLine(text);
      }
    }
  }

  public void SaveStates() {
    string text = JsonSerializer.Serialize(States);
    File.WriteAllText(Path, text);
  }

  public void LoadStates() {
    string text = File.ReadAllText(Path);
    States = JsonSerializer.Deserialize<List<PluginLoaderState>>(text ?? string.Empty) ?? new List<PluginLoaderState>();
  }

}
