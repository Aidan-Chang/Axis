using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace Axis.Web.Extension.Worker.Filters;

public class HangfireAuthorizeFilter : IDashboardAuthorizationFilter {
  public bool Authorize([NotNull] DashboardContext context) {
    return context.GetHttpContext().User.Identity?.IsAuthenticated ?? false;
  }
}

public static class HangfireAuthorizeExtension {
  public static bool IsReadOnly(this DashboardContext context, string role) {
    return context.GetHttpContext().User.IsInRole(role) == false;
  }
}