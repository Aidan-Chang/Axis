namespace Axis.Data.SqlBuilder;

public class UnsafeLiteral {

  public string Value { get; set; }

  public UnsafeLiteral(string value, bool replaceQuotes = true) {
    if (value == null) {
      value = "";
    }
    if (replaceQuotes) {
      value = value.Replace("'", "''");
    }
    Value = value;
  }

}
