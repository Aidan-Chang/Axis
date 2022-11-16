using Axis.Web.Extension.Common.Formats;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Net;

namespace Axis.Web.Extension.Common.Handlers;

[DebuggerStepThrough]
public static class ExceptionResultHandler {

  [DebuggerHidden]
  public static void UseExceptionResultHandler(this IApplicationBuilder app) {
    app.UseExceptionHandler(
      action => {
        action.Run(
          async context => {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature != null) {
              // convert message
              var value = contextFeature.Error.Message;
              // get the reqeust started time
              DateTimeOffset actionStartedOn = DateTimeOffset.UtcNow;
              if (context.Items.ContainsKey("x-action-started-on"))
                actionStartedOn = (DateTimeOffset)context.Items["x-action-started-on"]!;
              ResultData result = new() {
                StatusCode = context.Response.StatusCode,
                Success = false,
                TraceId = context.TraceIdentifier,
                Elapsed = (long)(DateTimeOffset.UtcNow - actionStartedOn).TotalMilliseconds,
                Error = new() {
                  Summary = contextFeature.Error.Message,
                  Details = contextFeature.Error.StackTrace,
                  Data = contextFeature.Error?.Data,
                }
              };
              // response formatted message
              await context.Response.WriteAsync(result.ToString());
            }
          });
      });
  }

}
