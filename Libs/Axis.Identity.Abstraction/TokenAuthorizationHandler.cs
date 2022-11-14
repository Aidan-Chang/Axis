using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Axis.Identity.Abstraction;

public class TokenAuthorizationHandler : AuthorizationHandler<TokenAuthorizationRequiremnt> {

  private readonly UserManager<IdentityUser<Guid>> _manager;

  public TokenAuthorizationHandler(UserManager<IdentityUser<Guid>> manager) {
    _manager = manager;
  }

  /// TODO: token: implement check in memory or radis
  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TokenAuthorizationRequiremnt requirement) {
    if (context.User.HasClaim(c => c.Type == requirement.TokenType)) {
      var user = await _manager.GetUserAsync(context.User);
      var claim = context.User.Claims.FirstOrDefault(x => x.Type == requirement.TokenType);
      var token = await _manager.GetAuthenticationTokenAsync(user, requirement.TokenProvider, claim!.Value);
      if (token != null) {
        context.Succeed(requirement);
      }
    }
  }

}
