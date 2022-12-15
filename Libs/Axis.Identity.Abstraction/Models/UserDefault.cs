namespace Axis.Identity.Abstraction.Models;

public class UserDefault {

  public Guid UserId { get; set; }

  public string Scope { get; set; } = string.Empty;

  public string Name { get; set; } = string.Empty;

  public string Value { get; set; } = string.Empty;

  public bool Enabled { get; set; } = true;

}
