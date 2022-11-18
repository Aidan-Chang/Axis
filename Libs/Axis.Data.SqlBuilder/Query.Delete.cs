namespace Axis.Data.SqlBuilder;

public partial class Query {

  public Query AsDelete() {
    Method = "delete";
    return this;
  }

}
