namespace Axis.Data.SqlBuilder.Clauses;

public abstract class AbstractFrom : AbstractClause {

  protected string _alias = default!;

  /// <summary>
  /// Try to extract the Alias for the current clause.
  /// </summary>
  /// <returns></returns>
  public virtual string Alias { get => _alias; set => _alias = value; }

}

/// <summary>
/// Represents a "from" clause.
/// </summary>
public class FromClause : AbstractFrom {

  public string Table { get; set; } = default!;

  public override string Alias {
    get {
      if (Table.IndexOf(" as ", StringComparison.OrdinalIgnoreCase) >= 0) {
        var segments = Table.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return segments[2];
      }
      return Table;
    }
  }

  /// <inheritdoc />
  public override AbstractClause Clone() {
    return new FromClause {
      Engine = Engine,
      Alias = Alias,
      Table = Table,
      Component = Component,
    };
  }

}

/// <summary>
/// Represents a "from subquery" clause.
/// </summary>
public class QueryFromClause : AbstractFrom {

  public Query Query { get; set; } = default!;

  public override string Alias {
    get {
      return string.IsNullOrEmpty(_alias) ? Query.QueryAlias : _alias;
    }
  }

  /// <inheritdoc />
  public override AbstractClause Clone() {
    return new QueryFromClause {
      Engine = Engine,
      Alias = Alias,
      Query = Query.Clone(),
      Component = Component,
    };
  }

}

public class RawFromClause : AbstractFrom {

  public string Expression { get; set; } = default!;

  public object[] Bindings { set; get; } = default!;

  /// <inheritdoc />
  public override AbstractClause Clone() {
    return new RawFromClause {
      Engine = Engine,
      Alias = Alias,
      Expression = Expression,
      Bindings = Bindings,
      Component = Component,
    };
  }

}

/// <summary>
/// Represents a FROM clause that is an ad-hoc table built with predefined values.
/// </summary>
public class AdHocTableFromClause : AbstractFrom {

  public List<string> Columns { get; set; } = default!;

  public List<object> Values { get; set; } = default!;

  public override AbstractClause Clone() {
    return new AdHocTableFromClause {
      Engine = Engine,
      Alias = Alias,
      Columns = Columns,
      Values = Values,
      Component = Component,
    };
  }

}