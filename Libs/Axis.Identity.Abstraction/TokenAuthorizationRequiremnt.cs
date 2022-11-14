using Microsoft.AspNetCore.Authorization;

namespace Axis.Identity.Abstraction;

public class TokenAuthorizationRequiremnt: IAuthorizationRequirement {

  public string TokenProvider { get; set; }

  public string TokenType { get; private set; }

  public TokenAuthorizationRequiremnt(string provider = "api", string tokenType = "jti") {
    TokenProvider = provider;
    TokenType = tokenType;
  }

}
