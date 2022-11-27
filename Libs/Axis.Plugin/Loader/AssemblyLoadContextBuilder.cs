using Axis.Plugin.Model;
using System.Reflection;
using System.Runtime.Loader;

namespace Axis.Plugin.Loader;

public class AssemblyLoadContextBuilder {

  private readonly List<string> _additionalProbingPaths = new();
  private readonly List<string> _resourceProbingPaths = new();
  private readonly List<string> _resourceProbingSubpaths = new();
  private readonly Dictionary<string, ManagedLibrary> _managedLibraries = new(StringComparer.Ordinal);
  private readonly Dictionary<string, NativeLibrary> _nativeLibraries = new(StringComparer.Ordinal);
  private readonly HashSet<string> _privateAssemblies = new(StringComparer.Ordinal);
  private readonly HashSet<string> _defaultAssemblies = new(StringComparer.Ordinal);
  private AssemblyLoadContext _defaultLoadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly()) ?? AssemblyLoadContext.Default;
  private string? _mainAssemblyPath;
  private bool _preferDefaultLoadContext;
  private bool _lazyLoadReferences;

  private bool _isCollectible;
  private bool _loadInMemory;
  private bool _shadowCopyNativeLibraries;

  public AssemblyLoadContext Build() {
    var resourceProbingPaths = new List<string>(_resourceProbingPaths);
    foreach (var additionalPath in _additionalProbingPaths) {
      foreach (var subPath in _resourceProbingSubpaths) {
        resourceProbingPaths.Add(Path.Combine(additionalPath, subPath));
      }
    }
    if (_mainAssemblyPath == null) {
      throw new InvalidOperationException($"Missing required property. You must call '{nameof(SetMainAssemblyPath)}' to configure the default assembly.");
    }
    return new ManagedLoadContext(
      _mainAssemblyPath,
      _managedLibraries,
      _nativeLibraries,
      _privateAssemblies,
      _defaultAssemblies,
      _additionalProbingPaths,
      resourceProbingPaths,
      _defaultLoadContext,
      _preferDefaultLoadContext,
      _lazyLoadReferences,
      _isCollectible,
      _loadInMemory,
      _shadowCopyNativeLibraries);
  }

  public AssemblyLoadContextBuilder SetMainAssemblyPath(string path) {
    if (string.IsNullOrEmpty(path)) {
      throw new ArgumentException("Argument must not be null or empty.", nameof(path));
    }
    if (!Path.IsPathRooted(path)) {
      throw new ArgumentException("Argument must be a full path.", nameof(path));
    }
    _mainAssemblyPath = path;
    return this;
  }

  public AssemblyLoadContextBuilder SetDefaultContext(AssemblyLoadContext context) {
    _defaultLoadContext = context ?? throw new ArgumentException($"Bad Argument: AssemblyLoadContext in {nameof(AssemblyLoadContextBuilder)}.{nameof(SetDefaultContext)} is null.");
    return this;
  }

  public AssemblyLoadContextBuilder PreferLoadContextAssembly(AssemblyName assemblyName) {
    if (assemblyName.Name != null) {
      _privateAssemblies.Add(assemblyName.Name);
    }
    return this;
  }

  public AssemblyLoadContextBuilder PreferDefaultLoadContextAssembly(AssemblyName assemblyName) {
    if (_lazyLoadReferences) {
      if (assemblyName.Name != null && !_defaultAssemblies.Contains(assemblyName.Name)) {
        _defaultAssemblies.Add(assemblyName.Name);
        var assembly = _defaultLoadContext.LoadFromAssemblyName(assemblyName);
        foreach (var reference in assembly.GetReferencedAssemblies()) {
          if (reference.Name != null) {
            _defaultAssemblies.Add(reference.Name);
          }
        }
      }
      return this;
    }
    var names = new Queue<AssemblyName>();
    names.Enqueue(assemblyName);
    while (names.TryDequeue(out var name)) {
      if (name.Name == null || _defaultAssemblies.Contains(name.Name)) {
        continue;
      }
      _defaultAssemblies.Add(name.Name);
      var assembly = _defaultLoadContext.LoadFromAssemblyName(name);
      foreach (var reference in assembly.GetReferencedAssemblies()) {
        names.Enqueue(reference);
      }
    }
    return this;
  }

  public AssemblyLoadContextBuilder PreferDefaultLoadContext(bool preferDefaultLoadContext) {
    _preferDefaultLoadContext = preferDefaultLoadContext;
    return this;
  }

  public AssemblyLoadContextBuilder IsLazyLoaded(bool isLazyLoaded) {
    _lazyLoadReferences = isLazyLoaded;
    return this;
  }

  public AssemblyLoadContextBuilder AddManagedLibrary(ManagedLibrary library) {
    ValidateRelativePath(library.AdditionalProbingPath);
    if (library.Name.Name != null) {
      _managedLibraries.Add(library.Name.Name, library);
    }
    return this;
  }

  public AssemblyLoadContextBuilder AddNativeLibrary(NativeLibrary library) {
    ValidateRelativePath(library.AppLocalPath);
    ValidateRelativePath(library.AdditionalProbingPath);
    _nativeLibraries.Add(library.Name, library);
    return this;
  }

  public AssemblyLoadContextBuilder AddProbingPath(string path) {
    if (string.IsNullOrEmpty(path)) {
      throw new ArgumentException("Value must not be null or empty.", nameof(path));
    }
    if (!Path.IsPathRooted(path)) {
      throw new ArgumentException("Argument must be a full path.", nameof(path));
    }
    _additionalProbingPaths.Add(path);
    return this;
  }

  public AssemblyLoadContextBuilder EnableUnloading() {
    _isCollectible = true;
    return this;
  }

  public AssemblyLoadContextBuilder PreloadAssembliesIntoMemory() {
    _loadInMemory = true;
    return this;
  }

  public AssemblyLoadContextBuilder ShadowCopyNativeLibraries() {
    _shadowCopyNativeLibraries = true;
    return this;
  }

  internal AssemblyLoadContextBuilder AddResourceProbingSubpath(string path) {
    if (string.IsNullOrEmpty(path)) {
      throw new ArgumentException("Value must not be null or empty.", nameof(path));
    }
    if (Path.IsPathRooted(path)) {
      throw new ArgumentException("Argument must be not a full path.", nameof(path));
    }
    _resourceProbingSubpaths.Add(path);
    return this;
  }

  private static void ValidateRelativePath(string probingPath) {
    if (string.IsNullOrEmpty(probingPath)) {
      throw new ArgumentException("Value must not be null or empty.", nameof(probingPath));
    }
    if (Path.IsPathRooted(probingPath)) {
      throw new ArgumentException("Argument must be a relative path.", nameof(probingPath));
    }
  }
}
