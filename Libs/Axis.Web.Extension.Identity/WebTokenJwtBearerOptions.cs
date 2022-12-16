using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Axis.Web.Extension.Identity;

public class WebTokenJwtBearerOptions : JwtBearerOptions {

  public string NameClaimType {
    get {
      if (TokenValidationParameters != null) {
        return TokenValidationParameters.NameClaimType;
      }
      return string.Empty;
    }
    set {
      if (TokenValidationParameters == null) {
        TokenValidationParameters = new TokenValidationParameters {
          NameClaimType = value,
          ValidIssuer = ValidIssuer,
          ValidAudience = Audience,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key))
        };
      }
      else {
        TokenValidationParameters.NameClaimType = value;
      }
    }
  }

  public string ValidIssuer {
    get {
      if (TokenValidationParameters != null) {
        return TokenValidationParameters.ValidIssuer;
      }
      return string.Empty;
    }
    set {
      if (TokenValidationParameters == null) {
        TokenValidationParameters = new TokenValidationParameters {
          NameClaimType = NameClaimType,
          ValidIssuer = value,
          ValidAudience = Audience,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key))
        };
      }
      else {
        TokenValidationParameters.NameClaimType = value;
      }
    }
  }

  private string _key = string.Empty;
  public string Key {
    get {
      return _key;
    }
    set {
      _key = value;
      if (TokenValidationParameters == null) {
        TokenValidationParameters = new TokenValidationParameters {
          NameClaimType = NameClaimType,
          ValidIssuer = ValidIssuer,
          ValidAudience = Audience,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key))
        };
      }
      else {
        TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
      }
    }
  }

}
