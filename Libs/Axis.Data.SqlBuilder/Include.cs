namespace Axis.Data.SqlBuilder;

public class Include {

  public string Name { get; set; } = default!;

  public Query Query { get; set; } = default!;

  public string? ForeignKey { get; set; } = default!;

  public string LocalKey { get; set; } = default!;

  public bool IsMany { get; set; }

}
