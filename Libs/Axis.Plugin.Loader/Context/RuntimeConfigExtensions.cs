﻿using Axis.Plugin.Loader.Internal;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Axis.Plugin.Loader.Context;

public static class RuntimeConfigExtensions {
  private const string JsonExt = ".json";
  private static readonly JsonSerializerOptions s_serializerOptions = new() {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public static AssemblyLoadContextBuilder TryAddAdditionalProbingPathFromRuntimeConfig(
    this AssemblyLoadContextBuilder builder,
    string runtimeConfigPath,
    bool includeDevConfig,
    out Exception? error) {
    error = null;
    try {
      var config = TryReadConfig(runtimeConfigPath);
      if (config == null) {
        return builder;
      }
      RuntimeConfig? devConfig = null;
      if (includeDevConfig) {
        var configDevPath = runtimeConfigPath[..^JsonExt.Length] + ".dev.json";
        devConfig = TryReadConfig(configDevPath);
      }
      var tfm = config.RuntimeOptions?.Tfm ?? devConfig?.RuntimeOptions?.Tfm;
      if (config.RuntimeOptions != null) {
        AddProbingPaths(builder, config.RuntimeOptions, tfm);
      }
      if (devConfig?.RuntimeOptions != null) {
        AddProbingPaths(builder, devConfig.RuntimeOptions, tfm);
      }
      if (tfm != null) {
        var dotnet = Process.GetCurrentProcess().MainModule?.FileName;
        if (dotnet != null && string.Equals(Path.GetFileNameWithoutExtension(dotnet), "dotnet", StringComparison.OrdinalIgnoreCase)) {
          var dotnetHome = Path.GetDirectoryName(dotnet);
          if (dotnetHome != null) {
            builder.AddProbingPath(Path.Combine(dotnetHome, "store", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(), tfm));
          }
        }
      }
    }
    catch (Exception ex) {
      error = ex;
    }
    return builder;
  }

  private static void AddProbingPaths(AssemblyLoadContextBuilder builder, RuntimeOptions options, string? tfm) {
    if (options.AdditionalProbingPaths == null) {
      return;
    }
    foreach (var item in options.AdditionalProbingPaths) {
      var path = item;
      if (path.Contains("|arch|")) {
        path = path.Replace("|arch|", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant());
      }
      if (path.Contains("|tfm|")) {
        if (tfm == null) {
          continue;
        }
        path = path.Replace("|tfm|", tfm);
      }
      builder.AddProbingPath(path);
    }
  }

  private static RuntimeConfig? TryReadConfig(string path) {
    try {
      var file = File.ReadAllBytes(path);
      return JsonSerializer.Deserialize<RuntimeConfig>(file, s_serializerOptions);
    }
    catch {
      return null;
    }
  }
}
