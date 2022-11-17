using Microsoft.AspNetCore.Builder;

namespace Axis.Message.RabbitMq;
public static class MessageQueueBuilderExtention {
  public static IApplicationBuilder UseRabbitMq(this IApplicationBuilder builder, Action<MessageQueueOptions> options) {
    MessageQueueOptions _options = new();
    options.Invoke(_options);
    /// TODO: rabbit mq: start a receiver, and stop on shutdown
    return builder;
  }
}
