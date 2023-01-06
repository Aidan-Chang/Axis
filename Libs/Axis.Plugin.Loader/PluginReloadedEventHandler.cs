namespace Axis.Plugin.Loader;

public delegate void PluginReloadedEventHandler(object sender, PluginReloadedEventArgs eventArgs);

public class PluginReloadedEventArgs : EventArgs {

  public string Name { get; set; }

  public string Version { get; set; }

  public PluginLoader Loader { get; }

  public PluginReloadedEventArgs(string name, string version, PluginLoader loader) {
    Name = name;
    Version = version;
    Loader = loader;
  }

}
