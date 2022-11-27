using System.Globalization;

namespace Axis.Data.Database.NamingConvention.Internal;

public class CamelCaseNameRewriter : INameRewriter {

  private readonly CultureInfo _culture;

  public CamelCaseNameRewriter(CultureInfo culture) => _culture = culture;

  public string RewriteName(string name) => string.IsNullOrEmpty(name) ? name : char.ToLower(name[0], _culture) + name[1..];

}
