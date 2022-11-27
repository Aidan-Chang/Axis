using Axis.Plugin.Abstractin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Axis.Plugin.AspNetCore;

public static class PluginExtension {

  // plugins
  private readonly static PluginOptions _options = new();
  private readonly static PluginLoaderCollection _loaders = new();
  private readonly static List<IWebPlugin> _plugins = new();

  public static IHostBuilder UsePluginLoader(this IHostBuilder builder, Action<PluginOptions> action) {
    if (builder == null) {
      throw new ArgumentNullException(nameof(builder));
    }
    if (action == null) {
      throw new ArgumentNullException("configureLogger");
    }
    // build options
    action?.Invoke(_options);
    // map to phycial path
    builder.ConfigureServices((ctx, services) => {
      if (_options.Path.ToLower().StartsWith(ctx.HostingEnvironment.ContentRootPath.ToLower()) == false) {
        _options.Path = Path.Combine(ctx.HostingEnvironment.ContentRootPath, _options.Path);
      }
    });
    // create directory
    if (Directory.Exists(_options.Path) == false) {
      Directory.CreateDirectory(_options.Path);
    }
    // get all assemblies
    foreach (var dir in new DirectoryInfo(_options.Path).GetDirectories(_options.Pattern, SearchOption.AllDirectories)) {
      foreach (var file in dir.GetFiles($"{dir.Name}.dll")) {
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
        // add loader to list
        _loaders[file.Name] = loader;
      }
    }
    return builder;
  }

  public static IMvcBuilder AddPlugins(this IMvcBuilder builder) {
    ILoggerFactory? loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();
    var logger = loggerFactory?.CreateLogger<PluginLoader>();
    // get all loaders
    foreach (var name in _loaders.Names) {
      var loader = _loaders[name];
      if (loader == null) {
        continue;
      }
      /// TODO: plugin: assembly hot reload & unloadable
      loader.Reloaded += (sender, e) => {
        logger?.LogInformation($"Plugins - Assembly reloaded: {e.Name}");
        var loader = (PluginLoader)sender;
        var assembly = loader.LoadDefaultAssembly();
        var factory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
        foreach (var part in factory.GetApplicationParts(assembly)) {
          builder.PartManager.ApplicationParts.Add(part);
        }
        //foreach (var type in assembly.GetTypes().Where(t => typeof(IWebPlugin).IsAssignableFrom(t) && !t.IsAbstract)) {
        //  var plugin = (IWebPlugin)Activator.CreateInstance(type)!;
        //  _plugins.Add(plugin);
        //  logger?.LogInformation($"Plugins - Assembly reloaded Found entry: {plugin.GetType().FullName}");
        //}
      };
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

  public static void UsePlugins(this IApplicationBuilder app) {
    ILoggerFactory? loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
    var logger = loggerFactory?.CreateLogger<PluginLoader>();
    // configure plugin service
    foreach (var plugin in _plugins) {
      plugin.Configure(app);
      logger?.LogInformation($"Plugins - Configured: {plugin.GetType().FullName}");
    }
  }

}
