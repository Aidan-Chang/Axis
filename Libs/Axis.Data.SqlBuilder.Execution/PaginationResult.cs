namespace Axis.Data.SqlBuilder.Execution;

public class PaginationResult<T> {

  public Query Query { get; set; } = default!;

  public long Total { get; set; }

  public IEnumerable<T> List { get; set; } = default!;

  public int Page => Skip / Size + 1;

  public int Skip { get; set; }

  public int Size { get; set; }

  //public int PerPage { get; set; }

  public int Pages {
    get {
      if (Size < 1)
        return 0;
      var div = (float)Total / Size;
      return (int)Math.Ceiling(div);
    }
  }

  //public bool IsFirst {
  //  get {
  //    return Page == 1;
  //  }
  //}

  //public bool IsLast {
  //  get {
  //    return Page == TotalPages;
  //  }
  //}

  //public bool HasNext {
  //  get {
  //    return Page < TotalPages;
  //  }
  //}

  //public bool HasPrevious {
  //  get {
  //    return Page > 1;
  //  }
  //}

  //public Query NextQuery() {
  //  return Query.ForPage(Page + 1, PerPage);
  //}

  //public PaginationResult<T> Next(IDbTransaction? transaction = null, int? timeout = null) {
  //  return Query.Paginate<T>(Page + 1, PerPage, transaction, timeout);
  //}

  //public async Task<PaginationResult<T>> NextAsync(IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
  //  return await Query.PaginateAsync<T>(Page + 1, PerPage, transaction, timeout, cancellationToken);
  //}

  //public Query PreviousQuery() {
  //  return Query.ForPage(Page - 1, PerPage);
  //}

  //public PaginationResult<T> Previous(IDbTransaction? transaction = null, int? timeout = null) {
  //  return Query.Paginate<T>(Page - 1, PerPage, transaction, timeout);
  //}

  //public async Task<PaginationResult<T>> PreviousAsync(IDbTransaction? transaction = null, int? timeout = null, CancellationToken cancellationToken = default) {
  //  return await Query.PaginateAsync<T>(Page - 1, PerPage, transaction, timeout, cancellationToken);
  //}

  //public PaginationIterator<T> Each {
  //  get {
  //    return new PaginationIterator<T> {
  //      FirstPage = this
  //    };
  //  }
  //}

}
