using Axis.Plugin.Abstractin;
using Axis.Plugin.Loader;
using Axis.Plugin.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Axis.Plugin;

public static class PluginExtension {

  // plugins
  private readonly static PluginOptions _options = new();
  private readonly static PluginCollection _list = new();
  private readonly static List<IWebPlugin> _plugins = new();

  public static IHostBuilder UsePlugins(this IHostBuilder builder, Action<PluginOptions> action) {
    if (builder == null) {
      throw new ArgumentNullException(nameof(builder));
    }
    if (action == null) {
      throw new ArgumentNullException("Plugin options is not configured");
    }
    // build options
    action?.Invoke(_options);
    // map to phycial path
    builder.ConfigureServices((ctx, services) => {
      if (_options.Path.ToLower().StartsWith(ctx.HostingEnvironment.ContentRootPath.ToLower()) == false) {
        _options.Path = Path.Combine(ctx.HostingEnvironment.ContentRootPath, _options.Path);
      }
      ILoggerFactory? loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
      var logger = loggerFactory?.CreateLogger<PluginLoader>();
      // create directory
      if (Directory.Exists(_options.Path) == false) {
        Directory.CreateDirectory(_options.Path);
      }
      // set loader base path && storage
      _list.BasePath = _options.Path;
      _list.Storage = new PluginLoaderFileStorage(_options.Path);
      _list.Load();
      // get all assemblies
      DirectoryInfo root = new(_list.BasePath);
      foreach (var dir in root.GetDirectories(_options.Pattern)) {
        foreach (var file in dir.GetFiles($"{dir.Name}.dll")) {
          // same name with dll file name and directory name
          if (dir.Name != file.Name.Replace(file.Extension, string.Empty)) {
            continue;
          }
          FileVersionInfo version = FileVersionInfo.GetVersionInfo(file.FullName);
          PluginEntry entry = _list[dir.Name] ?? new PluginEntry() {
            Name = dir.Name,
            Enabled = true,
          };
          entry.Version = version.FileVersion ?? "";
          if (entry.Enabled == true) {
            var loader = PluginLoader.CreateFromAssemblyFile(
              file.FullName,
              /// TODO: plugin: shared types
              //new Type[] { typeof(IServiceCollection), typeof(ILogger) },
              config => {
                config.PreferSharedTypes = _options.PreferSharedTypes;
                config.IsLazyLoaded = _options.IsLazyLoaded;
                config.IsUnloadable = _options.IsUnloadable;
                config.EnableHotReload = _options.EnableHotReload;
              });
            /// TODO: plugin: assembly hot reload & unloadable
            loader.Reloaded += (sender, e) => {
              if (_list != null) {
                PluginEntry? entry = _list[e.Name];
                if (entry != null) {
                  entry.Version = e.Version;
                  _list.Save();
                }
              }
              logger?.LogInformation($"Plugins reloaded - Assembly: {e.Name}, Version: {e.Version}");
            };
            entry.Loader = loader;
            // add loader to list
            _list[dir.Name] = entry;
          }
        }
      }
      // save to local file
      _list.Save();
    });
    return builder;
  }

  public static IMvcBuilder AddPluginServices(this IMvcBuilder builder) {
    ILoggerFactory? loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();
    var logger = loggerFactory?.CreateLogger<PluginLoader>();
    // get all loaders
    foreach (var name in _list.Names) {
      PluginEntry? info = _list[name];
      if (info == null) {
        continue;
      }
      PluginLoader? loader = info.Loader as PluginLoader;
      if (loader == null) {
        continue;
      }
      var assembly = loader.LoadDefaultAssembly();
      // load mvc application part from assemblies
      var factory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
      foreach (var part in factory.GetApplicationParts(assembly)) {
        builder.PartManager.ApplicationParts.Add(part);
      }
      // find and load related parts of assemblies
      var attributes = assembly.GetCustomAttributes<RelatedAssemblyAttribute>();
      foreach (var attribute in attributes) {
        var attribute_assembly = loader.LoadAssembly(attribute.AssemblyFileName);
        var attribute_factory = ApplicationPartFactory.GetApplicationPartFactory(attribute_assembly);
        foreach (var part in attribute_factory.GetApplicationParts(attribute_assembly)) {
          builder.PartManager.ApplicationParts.Add(part);
        }
      }
      // activator plugin
      foreach (var type in assembly.GetTypes().Where(t => typeof(IWebPlugin).IsAssignableFrom(t) && !t.IsAbstract)) {
        var plugin = (IWebPlugin)Activator.CreateInstance(type)!;
        _plugins.Add(plugin);
      }
      logger?.LogInformation($"Plugins - Added assembly: {name}");
    }
    // register plugin services
    foreach (var plugin in _plugins) {
      plugin.ConfigureServices(builder.Services, _options);
      logger?.LogInformation($"Plugins - Configured service: {plugin.GetType().FullName}");
    }
    // return builder
    return builder;
  }

  public static void UsePluginServices(this IApplicationBuilder app) {
    ILoggerFactory? loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
    var logger = loggerFactory?.CreateLogger<PluginLoader>();
    // configure plugin service
    foreach (var plugin in _plugins) {
      plugin.Configure(app);
      logger?.LogInformation($"Plugins - Configured: {plugin.GetType().FullName}");
    }
  }

}
