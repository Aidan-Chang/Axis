using Axis.Identity.Abstraction.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Axis.Identity.Common.Managers;

public class UserManager : UserManager<User> {

  private readonly IConfiguration _configuration;

  public UserManager(
    IUserStore<User> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<User> passwordHasher,
    IEnumerable<IUserValidator<User>> userValidators,
    IEnumerable<IPasswordValidator<User>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<User>> logger,
    IConfiguration configuration) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
    _configuration = configuration;
  }

  public override async Task<User?> FindByIdAsync(string userId) {
    userId = userId.ToLower();
    return await base.FindByIdAsync(userId);
  }

  public override async Task<string> GenerateUserTokenAsync(User user, string tokenProvider, string purpose) {
    // jwt identity
    string jti = Guid.NewGuid().ToString("D");
    // default claims
    var claims = new Dictionary<string, object> {
      { "userid", user.Id.ToString("D") },
      { "username", user.UserName ?? "" },
      { "title", user.Title },
      { "stamp", user.SecurityStamp??"" },
      { "jti", jti },
    };
    // add admin claim
    if (user.Admin) {
      claims.Add("admin", "true");
    }
    // config
    string key = _configuration["Jwt:Key"] ?? "";
    string issuer = _configuration["Jwt:Issuer"]?? "";
    string audience = _configuration["Jwt:Audience"]?? "";
    // generate token
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
    var expires = purpose switch {
      "client" => DateTimeOffset.UtcNow.AddDays(1),
      "resource" => DateTimeOffset.UtcNow.AddDays(7),
      _ => DateTimeOffset.UtcNow.AddHours(1),
    };
    // token descriptor
    var descriptor = new SecurityTokenDescriptor() {
      Issuer = issuer,
      Audience = audience,
      Claims = claims,
      NotBefore = DateTime.UtcNow,
      Expires = expires.UtcDateTime,
      SigningCredentials = credentials
    };
    // create token
    var token = new JsonWebTokenHandler().CreateToken(descriptor);
    // save to user tokens & return
    await SetAuthenticationTokenAsync(user, tokenProvider, jti, expires.UtcDateTime.ToString("yyyyMMddHHmmsss"));
    return token;
  }

}
