using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Plugin.Abstractin;

public interface IWebPlugin {

  string Name { get; }

  void ConfigureServices(IServiceCollection services, PluginOptions options = null!);

  void Configure(IApplicationBuilder appBuilder);

}
