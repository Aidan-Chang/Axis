namespace Axis.Data.SqlBuilder.Clauses;

public abstract class AbstractJoin : AbstractClause { }

public class BaseJoin : AbstractJoin {

  public Join Join { get; set; } = default!;

  public override AbstractClause Clone() {
    return new BaseJoin {
      Engine = Engine,
      Join = Join.Clone(),
      Component = Component,
    };
  }
}

public class DeepJoin : AbstractJoin {

  public string Type { get; set; } = default!;

  public string Expression { get; set; } = default!;

  public string SourceKeySuffix { get; set; } = default!;

  public string TargetKey { get; set; } = default!;

  public Func<string, string> SourceKeyGenerator { get; set; } = default!;

  public Func<string, string> TargetKeyGenerator { get; set; } = default!;

  /// <inheritdoc />
  public override AbstractClause Clone() {
    return new DeepJoin {
      Engine = Engine,
      Component = Component,
      Type = Type,
      Expression = Expression,
      SourceKeySuffix = SourceKeySuffix,
      TargetKey = TargetKey,
      SourceKeyGenerator = SourceKeyGenerator,
      TargetKeyGenerator = TargetKeyGenerator,
    };
  }

}
