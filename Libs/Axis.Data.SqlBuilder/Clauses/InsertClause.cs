namespace Axis.Data.SqlBuilder.Clauses;

public abstract class AbstractInsertClause : AbstractClause { }

public class InsertClause : AbstractInsertClause {

  public List<string> Columns { get; set; } = default!;

  public List<object> Values { get; set; } = default!;

  public bool ReturnId { get; set; } = false;

  public override AbstractClause Clone() {
    return new InsertClause {
      Engine = Engine,
      Component = Component,
      Columns = Columns,
      Values = Values,
      ReturnId = ReturnId,
    };
  }

}

public class InsertQueryClause : AbstractInsertClause {

  public List<string> Columns { get; set; } = default!;

  public Query Query { get; set; } = default!;

  public override AbstractClause Clone() {
    return new InsertQueryClause {
      Engine = Engine,
      Component = Component,
      Columns = Columns,
      Query = Query.Clone(),
    };
  }

}
