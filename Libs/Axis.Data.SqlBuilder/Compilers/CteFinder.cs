using Axis.Data.SqlBuilder.Clauses;

namespace Axis.Data.SqlBuilder.Compilers;

public class CteFinder {

  private readonly Query query = default!;
  private readonly string engineCode = default!;
  private HashSet<string> namesOfPreviousCtes = default!;
  private List<AbstractFrom> orderedCteList = default!;

  public CteFinder(Query query, string engineCode) {
    this.query = query;
    this.engineCode = engineCode;
  }

  public List<AbstractFrom> Find() {
    if (null != orderedCteList)
      return orderedCteList;
    namesOfPreviousCtes = new HashSet<string>();
    orderedCteList = findInternal(query);
    namesOfPreviousCtes.Clear();
    //namesOfPreviousCtes = null;
    return orderedCteList;
  }

  private List<AbstractFrom> findInternal(Query queryToSearch) {
    var cteList = queryToSearch.GetComponents<AbstractFrom>("cte", engineCode);
    var resultList = new List<AbstractFrom>();
    foreach (var cte in cteList) {
      if (namesOfPreviousCtes.Contains(cte.Alias))
        continue;
      namesOfPreviousCtes.Add(cte.Alias);
      resultList.Add(cte);
      if (cte is QueryFromClause queryFromClause)
        resultList.InsertRange(0, findInternal(queryFromClause.Query));
    }
    return resultList;
  }

}
