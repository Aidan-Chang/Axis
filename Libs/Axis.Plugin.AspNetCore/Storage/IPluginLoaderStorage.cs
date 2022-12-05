namespace Axis.Plugin.AspNetCore.Storage;

public interface IPluginLoaderStorage {

  public void Save(Dictionary<string, PluginInfo> collection);

  public Dictionary<string, PluginInfo> Load();

}
