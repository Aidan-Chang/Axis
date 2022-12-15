using Axis.Identity.Abstraction.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axis.Identity.Common.Managers;

public partial class SignInManager : SignInManager<User> {

  public SignInManager(
    UserManager manager,
    IHttpContextAccessor accessor,
    IUserClaimsPrincipalFactory<User> claims,
    IOptions<IdentityOptions> options,
    ILogger<SignInManager<User>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<User> confirmation) : base(manager, accessor, claims, options, logger, schemes, confirmation) { }

  public override async Task<SignInResult> PasswordSignInAsync(User user, string password, bool isPersistent, bool lockoutOnFailure) {
    var result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
    if (result.Succeeded) {
      // generate new token and set authentication token
      string token = await UserManager.GenerateUserTokenAsync(user, "api", "client");
      /// TODO: signin: set authentication would be here, for Single-responsiblity Principle (SRP)
      /// await SetAuthenticationTokenAsync(user, loginProvider, jti, expires.UtcDateTime.ToString("YYYYMMddHHmmsss"));
      Context.Items.Add("token", token);
    }
    return result;
  }

  public override async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor) {
    var result = await base.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
    if (result.Succeeded) {
      var user = await UserManager.FindByLoginAsync("ntlm", providerKey);
      if (user != null) {
        // generate new token and set authentication token
        string token = await UserManager.GenerateUserTokenAsync(user, "api", "client");
        /// TODO: signin: set authentication would be here, for Single-responsiblity Principle (SRP)
        /// await SetAuthenticationTokenAsync(user, loginProvider, jti, expires.UtcDateTime.ToString("YYYYMMddHHmmsss"));
        Context.Items.Add("token", token);
      }
    }
    return result;
  }

  public override async Task SignOutAsync() {
    var claim = Context.User.Claims.FirstOrDefault(x => x.Type == "jti");
    if (claim != null && Context?.User != null) {
      var user = await UserManager.GetUserAsync(Context.User);
      if (user != null) {
        await UserManager.RemoveAuthenticationTokenAsync(user, "api", claim.Value);
      }
    }
    await base.SignOutAsync();
  }

  public override async Task RefreshSignInAsync(User user) {
    await base.RefreshSignInAsync(user);
    // remove old token
    var claim = Context.User.Claims.FirstOrDefault(x => x.Type == "jti");
    if (claim != null) {
      await UserManager.RemoveAuthenticationTokenAsync(user, "api", claim.Value);
    }
    // generate new token
    string token = await UserManager.GenerateUserTokenAsync(user, "api", "client");
    Context.Items.Add("token", token);
  }

}
