namespace Axis.Plugin.Storage;

public interface IPluginLoaderStorage {

  public void Save(Dictionary<string, PluginEntry> collection);

  public Dictionary<string, PluginEntry> Load();

}
