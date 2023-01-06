using Axis.Plugin.Abstractin;
using Axis.Plugin.Loader.Context;
using Axis.Plugin.Loader.Internal;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json.Serialization;

namespace Axis.Plugin.Loader;

public class PluginLoader : IPluginLoader, IDisposable {

  public static PluginLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes)
    => CreateFromAssemblyFile(assemblyFile, isUnloadable, sharedTypes, _ => { });

  public static PluginLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes, Action<PluginConfig> configure) {
    return CreateFromAssemblyFile(
      assemblyFile,
      sharedTypes,
      config => {
        config.IsUnloadable = isUnloadable;
        configure(config);
      });
  }

  public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes)
    => CreateFromAssemblyFile(assemblyFile, sharedTypes, _ => { });

  public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes, Action<PluginConfig> configure) {
    return CreateFromAssemblyFile(
      assemblyFile,
      config => {
        if (sharedTypes != null) {
          var uniqueAssemblies = new HashSet<Assembly>();
          foreach (var type in sharedTypes) {
            uniqueAssemblies.Add(type.Assembly);
          }
          foreach (var assembly in uniqueAssemblies) {
            config.SharedAssemblies.Add(assembly.GetName());
          }
        }
        configure(config);
      });
  }

  public static PluginLoader CreateFromAssemblyFile(string assemblyFile)
    => CreateFromAssemblyFile(assemblyFile, _ => { });

  public static PluginLoader CreateFromAssemblyFile(string assemblyFile, Action<PluginConfig> configure) {
    if (configure == null) {
      throw new ArgumentNullException(nameof(configure));
    }
    var config = new PluginConfig(assemblyFile);
    configure(config);
    return new PluginLoader(config);
  }

  private readonly PluginConfig _config;
  private ManagedLoadContext _context;
  private readonly AssemblyLoadContextBuilder _contextBuilder;
  private volatile bool _disposed;

  private FileSystemWatcher? _fileWatcher;
  private Debouncer? _debouncer;

  public PluginLoader(PluginConfig config) {
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _contextBuilder = CreateLoadContextBuilder(config);
    _context = (ManagedLoadContext)_contextBuilder.Build();
    if (config.EnableHotReload) {
      StartFileWatcher();
    }
    Version = FileVersionInfo.GetVersionInfo(config.MainAssemblyPath).FileVersion ?? "";
  }

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string Version { get; set; } = string.Empty;

  [JsonInclude]
  public bool Enabled { get; set; } = true;

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public Dictionary<string, string>? Dependencies { get; set; } = null;

  public bool IsUnloadable => _context.IsCollectible;

  public event PluginReloadedEventHandler? Reloaded;

  public void Reload() {
    EnsureNotDisposed();
    if (!IsUnloadable) {
      throw new InvalidOperationException("Reload cannot be used because IsUnloadable is false");
    }
    _context.Unload();
    _context = (ManagedLoadContext)_contextBuilder.Build();
    GC.Collect();
    GC.WaitForPendingFinalizers();
    Version = FileVersionInfo.GetVersionInfo(_config.MainAssemblyPath).FileVersion ?? "";
    Reloaded?.Invoke(this,
      new PluginReloadedEventArgs(
        Path.GetFileNameWithoutExtension(_config.MainAssemblyPath),
        Version,
        this));
  }

  private void StartFileWatcher() {
    _debouncer = new Debouncer(_config.ReloadDelay);
    var watchedDir = Path.GetDirectoryName(_config.MainAssemblyPath);
    if (watchedDir == null) {
      throw new InvalidOperationException("Could not determine which directory to watch. Please set MainAssemblyPath to an absolute path so its parent directory can be discovered.");
    }
    _fileWatcher = new();
    _fileWatcher.Path = watchedDir;
    _fileWatcher.Changed += OnFileChanged;
    _fileWatcher.Filter = new FileInfo(_config.MainAssemblyPath).Name;
    _fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
    _fileWatcher.EnableRaisingEvents = true;
  }

  private void OnFileChanged(object source, FileSystemEventArgs e) {
    if (!_disposed) {
      _debouncer?.Execute(Reload);
    }
  }

  internal AssemblyLoadContext LoadContext => _context;

  public Assembly LoadDefaultAssembly() {
    EnsureNotDisposed();
    return _context.LoadAssemblyFromFilePath(_config.MainAssemblyPath);
  }

  public Assembly LoadAssembly(AssemblyName assemblyName) {
    EnsureNotDisposed();
    return _context.LoadFromAssemblyName(assemblyName);
  }

  public Assembly LoadAssemblyFromPath(string assemblyPath)
      => _context.LoadAssemblyFromFilePath(assemblyPath);

  public Assembly LoadAssembly(string assemblyName) {
    EnsureNotDisposed();
    return LoadAssembly(new AssemblyName(assemblyName));
  }

  public AssemblyLoadContext.ContextualReflectionScope EnterContextualReflection()
    => _context.EnterContextualReflection();

  public void Dispose() {
    if (_disposed) {
      return;
    }
    _disposed = true;
    if (_fileWatcher != null) {
      _fileWatcher.EnableRaisingEvents = false;
      _fileWatcher.Changed -= OnFileChanged;
      _fileWatcher.Dispose();
    }
    _debouncer?.Dispose();
    if (_context.IsCollectible) {
      _context.Unload();
    }
  }

  private void EnsureNotDisposed() {
    if (_disposed) {
      throw new ObjectDisposedException(nameof(PluginLoader));
    }
  }

  private static AssemblyLoadContextBuilder CreateLoadContextBuilder(PluginConfig config) {
    var builder = new AssemblyLoadContextBuilder();
    builder.SetMainAssemblyPath(config.MainAssemblyPath);
    builder.SetDefaultContext(config.DefaultContext);
    foreach (var ext in config.PrivateAssemblies) {
      builder.PreferLoadContextAssembly(ext);
    }
    if (config.PreferSharedTypes) {
      builder.PreferDefaultLoadContext(true);
    }
    if (config.IsUnloadable || config.EnableHotReload) {
      builder.EnableUnloading();
    }
    if (config.LoadInMemory) {
      builder.PreloadAssembliesIntoMemory();
      builder.ShadowCopyNativeLibraries();
    }
    builder.IsLazyLoaded(config.IsLazyLoaded);
    foreach (var assemblyName in config.SharedAssemblies) {
      builder.PreferDefaultLoadContextAssembly(assemblyName);
    }
    var baseDir = Path.GetDirectoryName(config.MainAssemblyPath);
    var assemblyFileName = Path.GetFileNameWithoutExtension(config.MainAssemblyPath);
    if (baseDir == null) {
      throw new InvalidOperationException("Could not determine which directory to watch. Please set MainAssemblyPath to an absolute path so its parent directory can be discovered.");
    }
    var pluginRuntimeConfigFile = Path.Combine(baseDir, assemblyFileName + ".runtimeconfig.json");
    builder.TryAddAdditionalProbingPathFromRuntimeConfig(pluginRuntimeConfigFile, includeDevConfig: true, out _);
    foreach (var runtimeconfig in Directory.GetFiles(AppContext.BaseDirectory, "*.runtimeconfig.json")) {
      builder.TryAddAdditionalProbingPathFromRuntimeConfig(runtimeconfig, includeDevConfig: true, out _);
    }
    return builder;
  }

}
