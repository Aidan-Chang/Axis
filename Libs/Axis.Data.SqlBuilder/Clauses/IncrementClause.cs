namespace Axis.Data.SqlBuilder.Clauses;

public class IncrementClause : InsertClause {

  public string Column { get; set; } = default!;

  public int Value { get; set; } = 1;

  public override AbstractClause Clone() {
    return new IncrementClause {
      Engine = Engine,
      Component = Component,
      Column = Column,
      Value = Value
    };
  }

}
