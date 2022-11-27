namespace Axis.Plugin;

public delegate void PluginReloadedEventHandler(object sender, PluginReloadedEventArgs eventArgs);

public class PluginReloadedEventArgs : EventArgs {

  public string Name { get; set; }

  public PluginLoader Loader { get; }

  public PluginReloadedEventArgs(string name, PluginLoader loader) {
    Name = name;
    Loader = loader;
  }

}
