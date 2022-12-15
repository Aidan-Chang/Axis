using Microsoft.AspNetCore.Identity;

namespace Axis.Identity.Abstraction.Models;

public class Role : IdentityRole<Guid> {

  public string GroupName { get; set; } = string.Empty;

  public int GroupNum { get; set; }

  public bool Admin { get; set; } = false;

  public bool Embedbed { get; set; } = false;

  public bool Enabled { get; set; } = true;

  public string CreatedBy { get; set; } = string.Empty;

  public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

  public string? UpdatedBy { get; set; }

  public DateTimeOffset? UpdatedOn { get; set; }

}
