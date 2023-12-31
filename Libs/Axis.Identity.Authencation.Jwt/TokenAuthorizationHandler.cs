﻿using Axis.Identity.Common.Managers;
using Microsoft.AspNetCore.Authorization;

namespace Axis.Identity.Authencation.Jwt;

public class TokenAuthorizationHandler : AuthorizationHandler<TokenAuthorizationRequiremnt> {

  private readonly UserManager _manager;

  public TokenAuthorizationHandler(UserManager manager) {
    _manager = manager;
  }

  /// TODO: token: implement check in memory or radis
  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TokenAuthorizationRequiremnt requirement) {
    if (context.User.HasClaim(c => c.Type == requirement.TokenType)) {
      var user = await _manager.GetUserAsync(context.User);
      var claim = context.User.Claims.FirstOrDefault(x => x.Type == requirement.TokenType);
      if (user == null || claim == null) {
        context.Fail(new(this, "User or Claim is not found"));
        return;
      }
      var token = await _manager.GetAuthenticationTokenAsync(user, requirement.TokenProvider, claim.Value);
      if (token != null) {
        context.Succeed(requirement);
      }
    }
  }

}
