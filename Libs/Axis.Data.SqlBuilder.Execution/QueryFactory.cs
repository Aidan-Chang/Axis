using Axis.Data.SqlBuilder.Clauses;
using Axis.Data.SqlBuilder.Compilers;
using Dapper;
using Humanizer;
using System.Data;
using System.Dynamic;

namespace Axis.Data.SqlBuilder.Execution;

//[DebuggerStepThrough]
public class QueryFactory : IDisposable {

  public IDbConnection Connection { get; set; } = default!;

  public Compiler Compiler { get; set; } = default!;

  public Action<SqlResult> Logger = result => { };

  private bool disposedValue;

  public int QueryTimeout { get; set; } = 30;

  public QueryFactory() { }

  public QueryFactory(IDbConnection connection, Compiler? compiler = null, int timeout = 30) {
    Connection = connection;
    if (compiler == null) {
      if (connection.GetType().GetProperty("WrappedConnection")?.GetValue(connection) is IDbConnection wrappedConnection) {
        connection = wrappedConnection;
      }
      compiler = connection.GetType().Name switch {
        "NpgsqlConnection" => new PostgresCompiler(),
        "SqlConnection" => new SqlServerCompiler(),
        "MySqlConnection" => new MySqlCompiler(),
        _ => throw new NotSupportedException($"Databast connection type \"{connection.GetType().Name}\" is not supported"),
      };
      Compiler = compiler;
    }
    else {
      Compiler = compiler;
    }
    QueryTimeout = timeout;
  }

  public Query Query() {
    var query = new XQuery(Connection, Compiler);
    query.QueryFactory = this;
    query.Logger = Logger;
    return query;
  }

  public Query Query(string table) {
    return Query().From(table);
  }

  /// <summary>
  /// Create an XQuery instance from a regular Query
  /// </summary>
  /// <param name="query"></param>
  /// <returns></returns>
  public Query FromQuery(Query query) {
    var xQuery = new XQuery(Connection, Compiler);
    xQuery.QueryFactory = this;
    xQuery.Clauses = query.Clauses.Select(x => x.Clone()).ToList();
    xQuery.SetParent(query.Parent);
    xQuery.QueryAlias = query.QueryAlias;
    xQuery.IsDistinct = query.IsDistinct;
    xQuery.Method = query.Method;
    xQuery.Includes = query.Includes;
    xQuery.Variables = query.Variables;
    xQuery.SetEngineScope(query.EngineScope);
    xQuery.Logger = Logger;
    return xQuery;
  }

  //[DebuggerHidden]
  public IEnumerable<T> Get<T>(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    var compiled = CompileAndLog(query);
    var result = Connection.Query<T>(
      compiled.Sql,
      compiled.NamedBindings,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout
    ).ToList();
    result = handleIncludes<T>(query, result).ToList();
    return result;
  }

  //[DebuggerHidden]
  public async Task<IEnumerable<T>> GetAsync<T>(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var compiled = CompileAndLog(query);
    var commandDefinition = new CommandDefinition(
      commandText: compiled.Sql,
      parameters: compiled.NamedBindings,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout,
      cancellationToken: cancellationToken);
    var result = (await Connection.QueryAsync<T>(commandDefinition)).ToList();
    result = (await handleIncludesAsync(query, result, cancellationToken)).ToList();
    return result;
  }

  public IEnumerable<IDictionary<string, object>> GetDictionary(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    var compiled = CompileAndLog(query);
    var result = Connection.Query(
      compiled.Sql,
      compiled.NamedBindings,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout
    );
    return result.Cast<IDictionary<string, object>>();
  }

  public async Task<IEnumerable<IDictionary<string, object>>> GetDictionaryAsync(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var compiled = CompileAndLog(query);
    var commandDefinition = new CommandDefinition(
      commandText: compiled.Sql,
      parameters: compiled.NamedBindings,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout,
      cancellationToken: cancellationToken);
    var result = await Connection.QueryAsync(commandDefinition);
    return result.Cast<IDictionary<string, object>>();
  }

  public IEnumerable<dynamic> Get(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    return Get<dynamic>(query, transaction, timeout);
  }

  public async Task<IEnumerable<dynamic>> GetAsync(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    return await GetAsync<dynamic>(query, transaction, timeout, cancellationToken);
  }

  public T FirstOrDefault<T>(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    var list = Get<T>(query.Limit(1), transaction, timeout);
    return list.ElementAtOrDefault(0)!;
  }

  public async Task<T> FirstOrDefaultAsync<T>(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var list = await GetAsync<T>(query.Limit(1), transaction, timeout, cancellationToken);
    return list.ElementAtOrDefault(0)!;
  }

  public dynamic? FirstOrDefault(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    return FirstOrDefault<dynamic>(query, transaction, timeout);
  }

  public async Task<dynamic?> FirstOrDefaultAsync(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    return await FirstOrDefaultAsync<dynamic>(query, transaction, timeout, cancellationToken);
  }

  public T First<T>(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    var item = FirstOrDefault<T>(query, transaction, timeout);
    if (item == null) {
      throw new InvalidOperationException("The sequence contains no elements");
    }
    return item;
  }

  public async Task<T> FirstAsync<T>(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var item = await FirstOrDefaultAsync<T>(query, transaction, timeout, cancellationToken);
    if (item == null) {
      throw new InvalidOperationException("The sequence contains no elements");
    }
    return item;
  }

  public dynamic First(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    return First<dynamic>(query, transaction, timeout);
  }

  public async Task<dynamic> FirstAsync(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    return await FirstAsync<dynamic>(query, transaction, timeout, cancellationToken);
  }

  public int Execute(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    var compiled = CompileAndLog(query);
    return Connection.Execute(
      compiled.Sql,
      compiled.NamedBindings,
      transaction,
      timeout ?? QueryTimeout
    );
  }

  public async Task<int> ExecuteAsync(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var compiled = CompileAndLog(query);
    var commandDefinition = new CommandDefinition(
      commandText: compiled.Sql,
      parameters: compiled.NamedBindings,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout,
      cancellationToken: cancellationToken);
    return await Connection.ExecuteAsync(commandDefinition);
  }

  public T ExecuteScalar<T>(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    var compiled = CompileAndLog(query.Limit(1));
    return Connection.ExecuteScalar<T>(
      compiled.Sql,
      compiled.NamedBindings,
      transaction,
      timeout ?? QueryTimeout
    );
  }

  public async Task<T> ExecuteScalarAsync<T>(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var compiled = CompileAndLog(query.Limit(1));
    var commandDefinition = new CommandDefinition(
      commandText: compiled.Sql,
      parameters: compiled.NamedBindings,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout,
      cancellationToken: cancellationToken);
    return await Connection.ExecuteScalarAsync<T>(commandDefinition);
  }

  public SqlMapper.GridReader GetMultiple<T>(Query[] queries, IDbTransaction? transaction = null, int? timeout = null) {
    var compiled = Compiler.Compile(queries);
    return Connection.QueryMultiple(
      compiled.Sql,
      compiled.NamedBindings,
      transaction,
      timeout ?? QueryTimeout
    );
  }

  public async Task<SqlMapper.GridReader> GetMultipleAsync<T>(Query[] queries, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var compiled = Compiler.Compile(queries);
    var commandDefinition = new CommandDefinition(
      commandText: compiled.Sql,
      parameters: compiled.NamedBindings,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout,
      cancellationToken: cancellationToken);
    return await Connection.QueryMultipleAsync(commandDefinition);
  }

  public IEnumerable<IEnumerable<T>> Get<T>(Query[] queries, IDbTransaction? transaction = null, int? timeout = null) {
    var multi = GetMultiple<T>(queries, transaction, timeout);
    using (multi)
      for (var i = 0; i < queries.Length; i++)
        yield return multi.Read<T>();
  }

  public async Task<IEnumerable<IEnumerable<T>>> GetAsync<T>(Query[] queries, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var multi = await GetMultipleAsync<T>(
      queries,
      transaction,
      timeout,
      cancellationToken
    );
    var list = new List<IEnumerable<T>>();
    using (multi) {
      for (var i = 0; i < queries.Length; i++) {
        list.Add(multi.Read<T>());
      }
    }
    return list;
  }

  public bool Exists(Query query, IDbTransaction? transaction = null, int? timeout = null) {
    var clone = query.Clone()
      .ClearComponent("select")
      .SelectRaw("1 as [Exists]")
      .Limit(1);
    var rows = Get(clone, transaction, timeout);
    return rows.Any();
  }

  public async Task<bool> ExistsAsync(Query query, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var clone = query.Clone()
      .ClearComponent("select")
      .SelectRaw("1 as [Exists]")
      .Limit(1);
    var rows = await GetAsync(clone, transaction, timeout, cancellationToken);
    return rows.Any();
  }

  public T Aggregate<T>(Query query, string aggregateOperation, string[]? columns = null, IDbTransaction? transaction = null, int? timeout = null) {
    return this.ExecuteScalar<T>(query.AsAggregate(aggregateOperation, columns), transaction, timeout ?? QueryTimeout);
  }

  public async Task<T> AggregateAsync<T>(Query query, string aggregateOperation, string[]? columns = null, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    return await this.ExecuteScalarAsync<T>(
      query.AsAggregate(aggregateOperation, columns),
      transaction,
      timeout,
      cancellationToken
    );
  }

  public T Count<T>(Query query, string[]? columns = null, IDbTransaction? transaction = null, int? timeout = null) {
    return this.ExecuteScalar<T>(
      query.AsCount(columns),
      transaction,
      timeout
    );
  }

  public async Task<T> CountAsync<T>(Query query, string[]? columns = null, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    return await this.ExecuteScalarAsync<T>(query.AsCount(columns), transaction, timeout, cancellationToken);
  }

  public T Average<T>(Query query, string column, IDbTransaction? transaction = null, int? timeout = null) {
    return Aggregate<T>(query, "avg", new[] { column });
  }

  public async Task<T> AverageAsync<T>(Query query, string column, CancellationToken cancellationToken = default) {
    return await AggregateAsync<T>(query, "avg", new[] { column }, cancellationToken: cancellationToken);
  }

  public T Sum<T>(Query query, string column) {
    return Aggregate<T>(query, "sum", new[] { column });
  }

  public async Task<T> SumAsync<T>(Query query, string column, CancellationToken cancellationToken = default) {
    return await AggregateAsync<T>(query, "sum", new[] { column }, cancellationToken: cancellationToken);
  }

  public T Min<T>(Query query, string column) {
    return Aggregate<T>(query, "min", new[] { column });
  }

  public async Task<T> MinAsync<T>(Query query, string column, CancellationToken cancellationToken = default) {
    return await AggregateAsync<T>(query, "min", new[] { column }, cancellationToken: cancellationToken);
  }

  public T Max<T>(Query query, string column) {
    return Aggregate<T>(query, "max", new[] { column });
  }

  public async Task<T> MaxAsync<T>(Query query, string column, CancellationToken cancellationToken = default) {
    return await AggregateAsync<T>(query, "max", new[] { column }, cancellationToken: cancellationToken);
  }

  public PaginationResult<T> Paginate<T>(Query query, int skip, int size = 25, IDbTransaction? transaction = null, int? timeout = null) {
    if (skip < 0) {
      throw new ArgumentException("Skip param should be greater than or equal to 0", nameof(skip));
    }
    if (size < 1) {
      throw new ArgumentException("Size param should be greater than or equal to 1", nameof(size));
    }
    var count = Count<long>(query.Clone(), null, transaction, timeout);
    IEnumerable<T> list;
    if (count > 0) {
      list = Get<T>(query.Clone().ForPage(skip, size), transaction, timeout);
    }
    else {
      list = Enumerable.Empty<T>();
    }
    return new PaginationResult<T> {
      Query = query,
      Size = size,
      Total = count,
      List = list,
      Skip = skip
    };
  }

  public async Task<PaginationResult<T>> PaginateAsync<T>(Query query, int skip, int size = 25, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    if (skip < 0) {
      throw new ArgumentException("Skip param should be greater than or equal to 0", nameof(skip));
    }
    if (size < 1) {
      throw new ArgumentException("Size param should be greater than or equal to 1", nameof(size));
    }
    var count = await CountAsync<long>(query.Clone(), null, transaction, timeout, cancellationToken);
    IEnumerable<T> list;
    if (count > 0) {
      list = await GetAsync<T>(query.Clone().ForPage(skip, size), transaction, timeout, cancellationToken);
    }
    else {
      list = Enumerable.Empty<T>();
    }
    return new PaginationResult<T> {
      Query = query,
      Size = size,
      Total = count,
      List = list,
      Skip = skip
    };
  }

  //public void Chunk<T>(Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func, IDbTransaction? transaction = null, int? timeout = null) {
  //  var result = Paginate<T>(query, 1, chunkSize, transaction, timeout);
  //  if (!func(result.List, 1))
  //    return;
  //  while (result.HasNext) {
  //    result = result.Next(transaction);
  //    if (!func(result.List, result.Page))
  //      return;
  //  }
  //}

  //public async Task ChunkAsync<T>(Query query, int chunkSize, Func<IEnumerable<T>, int, bool> func, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
  //  var result = await PaginateAsync<T>(query, 1, chunkSize, transaction, cancellationToken: cancellationToken);
  //  if (!func(result.List, 1)) {
  //    return;
  //  }
  //  while (result.HasNext) {
  //    result = result.Next(transaction);
  //    if (!func(result.List, result.Page)) {
  //      return;
  //    }
  //  }
  //}

  //public void Chunk<T>(Query query, int chunkSize, Action<IEnumerable<T>, int> action, IDbTransaction? transaction = null, int? timeout = null) {
  //  var result = Paginate<T>(query, 1, chunkSize, transaction, timeout);
  //  action(result.List, 1);
  //  while (result.HasNext) {
  //    result = result.Next(transaction);
  //    action(result.List, result.Page);
  //  }
  //}

  //public async Task ChunkAsync<T>(Query query, int chunkSize, Action<IEnumerable<T>, int> action, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
  //  var result = await PaginateAsync<T>(query, 1, chunkSize, transaction, timeout, cancellationToken);
  //  action(result.List, 1);
  //  while (result.HasNext) {
  //    result = result.Next(transaction);
  //    action(result.List, result.Page);
  //  }
  //}

  public IEnumerable<T> Select<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null) {
    return Connection.Query<T>(
      sql,
      param,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout
    );
  }

  public async Task<IEnumerable<T>> SelectAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var commandDefinition = new CommandDefinition(
      commandText: sql,
      parameters: param,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout,
      cancellationToken: cancellationToken);
    return await Connection.QueryAsync<T>(commandDefinition);
  }

  public IEnumerable<dynamic> Select(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null) {
    return Select<dynamic>(sql, param, transaction, timeout);
  }

  public async Task<IEnumerable<dynamic>> SelectAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    return await SelectAsync<dynamic>(sql, param, transaction, timeout, cancellationToken);
  }

  public int Statement(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null) {
    return Connection.Execute(sql, param, transaction: transaction, commandTimeout: timeout ?? QueryTimeout);
  }

  public async Task<int> StatementAsync(string sql, object? param = null, IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
    var commandDefinition = new CommandDefinition(
      commandText: sql,
      parameters: param,
      transaction: transaction,
      commandTimeout: timeout ?? QueryTimeout,
      cancellationToken: cancellationToken);
    return await Connection.ExecuteAsync(commandDefinition);
  }

  private static IEnumerable<T> handleIncludes<T>(Query query, IEnumerable<T> result) {
    if (!result.Any())
      return result;

    var canBeProcessed = query.Includes.Any() && result.ElementAt(0) is IDynamicMetaObjectProvider;
    if (!canBeProcessed)
      return result;

    var dynamicResult = result
      .Cast<IDictionary<string, object>>()
      .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
      .ToList();
    foreach (var include in query.Includes) {
      if (include.IsMany) {
        if (include.ForeignKey == null) {
          // try to guess the default key
          // I will try to fetch the table name if provided and appending the Id as a convention
          // Here am using Humanizer package to help getting the singular form of the table
          if (query.GetOneComponent("from") is not FromClause fromTable) {
            throw new InvalidOperationException($"Cannot guess the foreign key for the included relation '{include.Name}'");
          }
          var table = fromTable.Alias ?? fromTable.Table;
          include.ForeignKey = table.Singularize(false) + "Id";
        }
        var localIds = dynamicResult.Where(x => x[include.LocalKey] != null)
        .Select(x => x[include.LocalKey].ToString())
        .ToList();
        if (!localIds.Any()) {
          continue;
        }
        var children = include
          .Query
          .WhereIn(include.ForeignKey, localIds)
          .Get()
          .Cast<IDictionary<string, object>>()
          .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
          .GroupBy(x => x[include.ForeignKey].ToString())
          .ToDictionary(x => x.Key!, x => x.ToList());

        foreach (var item in dynamicResult) {
          var localValue = item[include.LocalKey].ToString()!;
          item[include.Name] = children.ContainsKey(localValue) ? children[localValue] : new List<Dictionary<string, object>>();
        }
        continue;
      }

      include.ForeignKey ??= include.Name + "Id";

      var foreignIds = dynamicResult
        .Where(x => x[include.ForeignKey] != null)
        .Select(x => x[include.ForeignKey].ToString())
        .ToList();

      if (!foreignIds.Any()) {
        continue;
      }
      var related = include
        .Query
        .WhereIn(include.LocalKey, foreignIds)
        .Get()
        .Cast<IDictionary<string, object>>()
        .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
        .ToDictionary(x => x[include.LocalKey].ToString()!);

      foreach (var item in dynamicResult) {
        var foreignValue = item[include.ForeignKey]?.ToString();
        item[include.Name] = foreignValue != null && related.ContainsKey(foreignValue) ? related[foreignValue] : null!;
      }
    }
    return dynamicResult.Cast<T>();
  }

  private static async Task<IEnumerable<T>> handleIncludesAsync<T>(Query query, IEnumerable<T> result, CancellationToken cancellationToken = default) {
    if (!result.Any()) {
      return result;
    }
    var canBeProcessed = query.Includes.Any() && result.ElementAt(0) is IDynamicMetaObjectProvider;
    if (!canBeProcessed) {
      return result;
    }
    var dynamicResult = result
      .Cast<IDictionary<string, object>>()
      .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
      .ToList();

    foreach (var include in query.Includes) {
      if (include.IsMany) {
        if (include.ForeignKey == null) {
          // try to guess the default key
          // I will try to fetch the table name if provided and appending the Id as a convention
          // Here am using Humanizer package to help getting the singular form of the table
          if (query.GetOneComponent("from") is not FromClause fromTable) {
            throw new InvalidOperationException($"Cannot guess the foreign key for the included relation '{include.Name}'");
          }
          var table = fromTable.Alias ?? fromTable.Table;
          include.ForeignKey = table.Singularize(false) + "Id";
        }
        var localIds = dynamicResult.Where(x => x[include.LocalKey] != null)
          .Select(x => x[include.LocalKey].ToString())
          .ToList();
        if (!localIds.Any()) {
          continue;
        }
        var children = (await include.Query.WhereIn(include.ForeignKey, localIds).GetAsync(cancellationToken: cancellationToken))
          .Cast<IDictionary<string, object>>()
          .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
          .GroupBy(x => x[include.ForeignKey].ToString())
          .ToDictionary(x => x.Key!, x => x.ToList());
        foreach (var item in dynamicResult) {
          var localValue = item[include.LocalKey].ToString()!;
          item[include.Name] = children.ContainsKey(localValue) ? children[localValue] : new List<Dictionary<string, object>>();
        }
        continue;
      }
      include.ForeignKey ??= include.Name + "Id";
      var foreignIds = dynamicResult.Where(x => x[include.ForeignKey] != null)
        .Select(x => x[include.ForeignKey].ToString())
        .ToList();
      if (!foreignIds.Any()) {
        continue;
      }
      var related = (await include.Query.WhereIn(include.LocalKey, foreignIds).GetAsync(cancellationToken: cancellationToken))
        .Cast<IDictionary<string, object>>()
        .Select(x => new Dictionary<string, object>(x, StringComparer.OrdinalIgnoreCase))
        .ToDictionary(x => x[include.LocalKey].ToString()!);

      foreach (var item in dynamicResult) {
        var foreignValue = item[include.ForeignKey].ToString();
        item[include.Name] = foreignValue != null && related.ContainsKey(foreignValue!) ? related[foreignValue] : null!;
      }
    }
    return dynamicResult.Cast<T>();
  }

  /// <summary>
  /// Compile and log query
  /// </summary>
  /// <param name="query"></param>
  /// <returns></returns>
  internal SqlResult CompileAndLog(Query query) {
    var compiled = Compiler.Compile(query);
    Logger(compiled);
    return compiled;
  }

  protected virtual void Dispose(bool disposing) {
    if (!disposedValue) {
      if (disposing) {
        Connection.Dispose();
      }
      // TODO: free unmanaged resources (unmanaged objects) and override finalizer
      // TODO: set large fields to null
      Connection = null!;
      Compiler = null!;
      disposedValue = true;
    }
  }

  // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
  // ~QueryFactory()
  // {
  //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
  //     Dispose(disposing: false);
  // }

  public void Dispose() {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

}
