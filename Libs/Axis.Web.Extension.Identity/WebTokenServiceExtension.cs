using Axis.Identity.Abstraction.Models;
using Axis.Identity.Authencation.Jwt;
using Axis.Identity.Common.DbContexts;
using Axis.Identity.Common.Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Web.Extension.Identity;

public static class WebTokenServiceExtension {

  public static IServiceCollection AddWebToken(
    this IServiceCollection services,
    Action<IdentityOptions> options,
    Action<WebTokenJwtBearerOptions> options2) {
    // Add identity
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

    // Add authentication
    services
      .AddAuthentication(options => {
        options.DefaultScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
      })
      .AddJwtBearer((Action<JwtBearerOptions>)options2);

    // Add authorization
    services
      .AddAuthorization(options => {
        AuthorizationPolicyBuilder policy = new("Bearer");
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("jti");
        policy.AddRequirements(new TokenAuthorizationRequiremnt());
        options.DefaultPolicy = policy.Build();
      });

    // Block the logged out token
    services.AddScoped<IAuthorizationHandler, TokenAuthorizationHandler>();

    return services;
  }

}
