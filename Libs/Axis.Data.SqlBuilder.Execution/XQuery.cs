using Axis.Data.SqlBuilder.Compilers;
using System.Data;

namespace Axis.Data.SqlBuilder.Execution;

public class XQuery : Query {

  public IDbConnection Connection { get; set; }

  public Compiler Compiler { get; set; }

  public Action<SqlResult> Logger = result => { };

  public QueryFactory QueryFactory { get; set; } = default!;

  public XQuery(IDbConnection connection, Compiler compiler) {
    QueryFactory = new QueryFactory(connection, compiler);
    Connection = connection;
    Compiler = compiler;
  }

  public override Query Clone() {
    var query = new XQuery(QueryFactory.Connection, QueryFactory.Compiler);
    query.QueryFactory.QueryTimeout = QueryFactory?.QueryTimeout ?? 30;
    query.Clauses = this.Clauses.Select(x => x.Clone()).ToList();
    query.Logger = Logger;
    query.QueryAlias = QueryAlias;
    query.IsDistinct = IsDistinct;
    query.Method = Method;
    query.Includes = Includes;
    query.Variables = Variables;
    query.SetEngineScope(EngineScope);
    return query;
  }

}

