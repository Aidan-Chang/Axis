using Axis.Plugin.Abstractin;
using Axis.Plugin.Loader;
using Axis.Plugin.Storage;
using Axis.Plugin.Storage.FileStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Axis.Plugin;

public static class PluginExtension {

  private readonly static PluginCollection _list = new();

  public static IHostBuilder UsePlugins(this IHostBuilder builder, Action<PluginOptions> action) {
    if (builder == null) {
      throw new ArgumentNullException(nameof(builder));
    }
    if (action == null) {
      throw new ArgumentNullException("Plugin options is not configured");
    }
    // map to phycial path
    builder.ConfigureServices((ctx, services) => {
      PluginOptions options = new PluginOptions();
      action(options);
      services.TryAddSingleton(options);
      if (options.Path.ToLower().StartsWith(ctx.HostingEnvironment.ContentRootPath.ToLower()) == false) {
        options.Path = Path.Combine(ctx.HostingEnvironment.ContentRootPath, options.Path);
      }
      ILogger? logger = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger<PluginLoader>();
      // create directory
      if (Directory.Exists(options.Path) == false) {
        Directory.CreateDirectory(options.Path);
      }
      // set loader base path && storage
      //services.TryAddSingleton<IPluginLoaderStorage, PluginLoaderFileStorage>();
      //_list.Storage = storage;
      _list.Storage = options.Storage.Trim() switch {
        "File" => new PluginLoaderFileStorage(options),
        "" => throw new ArgumentException($"Plugin storage is not provided"),
        _ => throw new ArgumentException($"Plugin storage - {options.Storage} is not Supported")
      };
      _list.BasePath = options.Path;
      _list.Pattern = options.Pattern;
      _list.Load();
      // find all assemblies and update entry
      _list.FindAndUpdateFromPluginAssembly((entry) => {
        logger?.LogInformation($"Plugins found - Assembly: {entry.Path}, Version: {entry.Version}");
        var loader = PluginLoader.CreateFromAssemblyFile(
          entry.Path,
          /// TODO: plugin: shared types
          //new Type[] { typeof(IServiceCollection), typeof(ILogger) },
          config => {
            config.PreferSharedTypes = options.PreferSharedTypes;
            config.IsLazyLoaded = options.IsLazyLoaded;
            config.IsUnloadable = options.IsUnloadable;
            config.EnableHotReload = options.EnableHotReload;
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
        return loader;
      });
      // save to local file
      _list.Save();
    });
    return builder;
  }

  public static IMvcBuilder AddPluginServices(this IMvcBuilder builder) {
    ILogger? logger = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger<PluginLoader>();
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
      // activator plugin & configure
      foreach (var type in assembly.GetTypes().Where(t => typeof(IWebPlugin).IsAssignableFrom(t) && !t.IsAbstract)) {
        var plugin = (IWebPlugin)Activator.CreateInstance(type)!;
        if (info.WebPlugins == null)
          info.WebPlugins = new List<IWebPlugin>();
        info.WebPlugins?.Add(plugin);
        logger?.LogInformation($"Plugins - Added web plugin - {plugin.GetType().FullName} from assembly: {name}");
        // configure plugin service
        plugin.ConfigureServices(builder.Services);
        logger?.LogInformation($"Plugins - Configured plugin service: {plugin.GetType().FullName}");
      }
    }
    // return builder
    return builder;
  }

  public static void UsePluginServices(this IApplicationBuilder app) {
    ILogger? logger = app.ApplicationServices.GetService<ILoggerFactory>()?.CreateLogger<PluginLoader>();
    foreach (var name in _list.Names) {
      PluginEntry? info = _list[name];
      if (info == null || info.WebPlugins == null) continue;
      foreach (var plugin in info.WebPlugins) {
        if (plugin != null) {
          // configure plugin
          plugin.Configure(app);
          logger?.LogInformation($"Plugins - Configured plugin: {plugin.GetType().FullName}");
        }
      }
    }
  }

}
