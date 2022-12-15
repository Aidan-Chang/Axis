using Microsoft.AspNetCore.Identity;

namespace Axis.Identity.Abstraction.Models;

public class User : IdentityUser<Guid> {

  public string Title { get; set; } = string.Empty;

  public DateTimeOffset? LastLogOn { get; set; }

  public DateTimeOffset? TokenRefreshOn { get; set; }

  public bool Admin { get; set; } = false;

  public bool Embedbed { get; set; } = false;

  public bool Enabled { get; set; } = true;

  public string CreatedBy { get; set; } = string.Empty;

  public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

  public string? UpdatedBy { get; set; }

  public DateTimeOffset? UpdatedOn { get; set; }

}
