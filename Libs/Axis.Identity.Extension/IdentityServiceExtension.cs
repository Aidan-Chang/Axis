using Axis.Identity.Abstraction.Models;
using Axis.Identity.Common.DbContexts;
using Axis.Identity.Common.Managers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Identity.Extension;
public static class IdentityServiceExtension {

  public static IServiceCollection AddIdentity(this IServiceCollection services, Action<IdentityOptions> options) {
    services
      .AddDbContextPool<IdentityDbContext>((provider, builder)
        => provider.GetRequiredService<Action<DbContextOptionsBuilder>>().Invoke(builder))
      .AddIdentity<User, Role>(options)
      .AddEntityFrameworkStores<IdentityDbContext>()
      .AddUserManager<UserManager>()
      .AddRoleManager<RoleManager>()
      .AddRoleValidator<RoleValidator>()
      .AddSignInManager<SignInManager>()
      .AddDefaultTokenProviders();
    return services;
  }

}
