using System.Globalization;

namespace Axis.Data.Database.NamingConvention.Internal;

public class UpperSnakeCaseNameRewriter : SnakeCaseNameRewriter {

  private readonly CultureInfo _culture;

  public UpperSnakeCaseNameRewriter(CultureInfo culture) : base(culture) => _culture = culture;

  public override string RewriteName(string name) => base.RewriteName(name).ToUpper(_culture);

}
