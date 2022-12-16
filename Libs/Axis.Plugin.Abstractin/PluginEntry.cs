using System.Text.Json.Serialization;

namespace Axis.Plugin.Abstractin;

public class PluginEntry {

  public string Name { get; set; } = string.Empty;

  public string Version { get; set; } = string.Empty;

  public bool Enabled { get; set; } = true;

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public Dictionary<string, string>? Dependencies { get; set; }

  [JsonIgnore]
  public IPluginLoader? Loader { get; set; }

}
