using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Axis.Message.SignalR.Hubs;

[Authorize]
public class MessageHub : Hub {

  public static IHubContext<MessageHub> GlobalContext { get; private set; } = null!;

  public MessageHub(IHubContext<MessageHub> context) {
    GlobalContext = context;
  }

  public Task Send(string method, object message, CancellationToken token) {
    return Clients.All.SendAsync(method, message, token);
  }

  public Task JoinGroup(string name, CancellationToken token) {
    return Groups.AddToGroupAsync(Context.ConnectionId, name, token);
  }

  public Task SendToGroup(string name, string method, object message, CancellationToken token) {
    return Clients.Group(name).SendAsync(method, message, token);
  }

  public Task RemoveGroup(string name, CancellationToken token) {
    return Groups.RemoveFromGroupAsync(Context.ConnectionId, name, token);
  }

}
