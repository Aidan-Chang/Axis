using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace Axis.Message.SignalR.Hubs;

[DebuggerStepThrough]
[AllowAnonymous]
public sealed class ProgressHub : Hub {

  [DebuggerHidden]
  public Task Cancel(string id) {
    if (string.IsNullOrEmpty(id) == false) {
      return Task.Run(
        () => {
          HubContextCancelledCollection.AddCancelledRequest(id);
        });
    }
    else {
      return Task.CompletedTask;
    }
  }

}

[DebuggerStepThrough]
internal static class HubContextCancelledCollection {

  private static readonly Dictionary<string, DateTimeOffset> _cancelledRequests = new();

  [DebuggerHidden]
  public static void AddCancelledRequest(string id) {
    if (_cancelledRequests.TryAdd(id, DateTimeOffset.UtcNow) == false) {
      _cancelledRequests[id] = DateTimeOffset.UtcNow;
    }
    // remove exipred cancellable records
    var expired_ids = _cancelledRequests.Where(x => x.Value < DateTimeOffset.UtcNow.AddDays(-1)).Select(x => x.Key);
    foreach (var expired_id in expired_ids) {
      _cancelledRequests.Remove(expired_id);
    }
  }

  [DebuggerHidden]
  public static bool ExistsCancelledRequest(this IHubContext<ProgressHub> _, string id) {
    return _cancelledRequests.ContainsKey(id);
  }

  [DebuggerHidden]
  public static void RemoveCancelledRequest(this IHubContext<ProgressHub> _, string id) {
    _cancelledRequests.Remove(id);
  }

}