using Axis.Data.Database.NamingConvention.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Data.Database.NamingConvention;

public static class ServiceCollectionExtensions {
  public static IServiceCollection AddEntityFrameworkNamingConventions(
    [NotNull] this IServiceCollection serviceCollection) {
    Check.NotNull(serviceCollection, nameof(serviceCollection));
    new EntityFrameworkServicesBuilder(serviceCollection)
      .TryAdd<IConventionSetPlugin, NamingConventionSetPlugin>();
    return serviceCollection;
  }
}