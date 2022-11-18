using Axis.Data.SqlBuilder.Clauses;
using System.Text.RegularExpressions;

namespace Axis.Data.SqlBuilder.Compilers;

public class FirebirdCompiler : Compiler {

  public FirebirdCompiler() { }

  public override string EngineCode { get; } = EngineCodes.Firebird;

  protected override string SingleRowDummyTableName => "RDB$DATABASE";

  protected override SqlResult CompileInsertQuery(Query query) {
    var ctx = base.CompileInsertQuery(query);
    var inserts = ctx.Query.GetComponents<AbstractInsertClause>("insert", EngineCode);
    if (inserts.Count > 1) {
      ctx.RawSql = Regex.Replace(ctx.RawSql, @"\)\s+VALUES\s+\(", ") SELECT ");
      ctx.RawSql = Regex.Replace(ctx.RawSql, @"\),\s*\(", " FROM RDB$DATABASE UNION ALL SELECT ");
      ctx.RawSql = Regex.Replace(ctx.RawSql, @"\)$", " FROM RDB$DATABASE");
    }
    return ctx;
  }

  public override string CompileLimit(SqlResult ctx) {
    var limit = ctx.Query.GetLimit(EngineCode);
    var offset = ctx.Query.GetOffset(EngineCode);
    if (limit > 0 && offset > 0) {
      ctx.Bindings.Add(offset + 1);
      ctx.Bindings.Add(limit + offset);
      return "ROWS ? TO ?";
    }
    return null!;
  }


  protected override string CompileColumns(SqlResult ctx) {
    var compiled = base.CompileColumns(ctx);
    var limit = ctx.Query.GetLimit(EngineCode);
    var offset = ctx.Query.GetOffset(EngineCode);
    if (limit > 0 && offset == 0) {
      ctx.Bindings.Insert(0, limit);
      ctx.Query.ClearComponent("limit");
      return "SELECT FIRST ?" + compiled[6..];
    }
    else if (limit == 0 && offset > 0) {
      ctx.Bindings.Insert(0, offset);
      ctx.Query.ClearComponent("offset");
      return "SELECT SKIP ?" + compiled[6..];
    }
    return compiled;
  }

  protected override string CompileBasicDateCondition(SqlResult ctx, BasicDateCondition condition) {
    var column = Wrap(condition.Column);
    string left = condition.Part switch {
      "time" => $"CAST({column} as TIME)",
      "date" => $"CAST({column} as DATE)",
      _ => $"EXTRACT({condition.Part.ToUpperInvariant()} FROM {column})",
    };
    var sql = $"{left} {condition.Operator} {Parameter(ctx, condition.Value)}";
    if (condition.IsNot) {
      return $"NOT ({sql})";
    }
    return sql;
  }

  public override string WrapValue(string value) {
    return base.WrapValue(value).ToUpperInvariant();
  }

  public override string CompileTrue() {
    return "1";
  }

  public override string CompileFalse() {
    return "0";
  }

}
