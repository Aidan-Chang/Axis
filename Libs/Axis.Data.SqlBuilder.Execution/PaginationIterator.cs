//using System.Collections;

//namespace Axis.Data.SqlBuilder.Execution;

//public class PaginationIterator<T> : IEnumerable<PaginationResult<T>> {

//  public PaginationResult<T> FirstPage { get; set; } = default!;

//  public PaginationResult<T> CurrentPage { get; set; } = default!;

//  public IEnumerator<PaginationResult<T>> GetEnumerator() {
//    CurrentPage = FirstPage;
//    yield return CurrentPage;
//    while (CurrentPage.HasNext) {
//      CurrentPage = CurrentPage.Next();
//      yield return CurrentPage;
//    }
//  }

//  IEnumerator IEnumerable.GetEnumerator() {
//    return GetEnumerator();
//  }

//}
