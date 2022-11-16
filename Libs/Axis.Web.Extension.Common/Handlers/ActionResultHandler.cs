using Axis.Web.Extension.Common.Formats;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Axis.Web.Extension.Common.Handlers;

[DebuggerStepThrough]
public class ActionResultHandler : IAsyncActionFilter {

  private readonly ILogger _logger;

  public ActionResultHandler(ILogger logger) {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  [DebuggerHidden]
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
    // begin execution
    _logger.LogInformation($"Action start: {context.HttpContext.TraceIdentifier}, User: {context.HttpContext.User}, Url: {context.HttpContext.Request.GetDisplayUrl()}");
    Stopwatch stopWatch = new();
    stopWatch.Start();
    // set now to the header, calculate the execution period when get exception.
    context.HttpContext.Items.Add("x-action-started-on", DateTimeOffset.UtcNow);
    // execution
    var action = await next();
    // end execution
    stopWatch.Stop();
    _logger.LogInformation($"Action end: {context.HttpContext.TraceIdentifier}, elapsed: {stopWatch.ElapsedMilliseconds}");
    // get status code
    var statusCode =
      context.HttpContext.RequestAborted.IsCancellationRequested ?
      499 :
      (action.Result as ObjectResult)?.StatusCode ??
      (action.Result as StatusCodeResult)?.StatusCode ??
      200;
    var value = (action.Result as ObjectResult)?.Value;
    // result data factory
    Func<int, bool, object?, string?, ResultData> result = (statusCode, success, data, message) => {
      ResultData resultData = new() {
        StatusCode = statusCode,
        Elapsed = stopWatch.ElapsedMilliseconds,
        TraceId = context.HttpContext.TraceIdentifier,
      };
      resultData.Success = success;
      if (string.IsNullOrEmpty(message) == false) {
        resultData.Error = new() {
          Summary = message,
          Details = null,
          Data = data,
        };
      }
      else {
        resultData.Data = data;
      }
      return resultData;
    };
    // action return with result data
    action.Result = action.Result switch {
      NotFoundResult or NotFoundObjectResult
        => new NotFoundObjectResult(result.Invoke(statusCode, false, null, value?.ToString())),
      ConflictResult or ConflictObjectResult
        => new ConflictObjectResult(result.Invoke(statusCode, false, null, value?.ToString())),
      BadRequestResult or BadRequestObjectResult
        => new BadRequestObjectResult(result.Invoke(statusCode, false, null, value?.ToString())),
      UnauthorizedResult or UnauthorizedObjectResult
        => new UnauthorizedObjectResult(result.Invoke(statusCode, false, null, value?.ToString())),
      null
        => new ObjectResult(result.Invoke(499, false, null, action.Exception?.Message ?? "The operation was canceled.")),
      _
        => new ObjectResult(result.Invoke(statusCode, true, value, null)),
    };
  }

}