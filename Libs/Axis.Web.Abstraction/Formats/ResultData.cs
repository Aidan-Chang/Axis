using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axis.Web.Abstraction.Formats;

[DebuggerStepThrough]
public class ResultData {

  public int StatusCode { get; set; } = 0;

  public bool Success { get; set; }

  public string TraceId { get; set; } = string.Empty;

  public long Elapsed { get; set; } = 0;

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public object? Data { get; set; } = null;

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public Message Error { get; set; } = null!;

  public override string ToString() => JsonSerializer.Serialize(this);

}


[DebuggerStepThrough]
public class Message {

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? Summary { get; set; } = string.Empty;

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? Details { get; set; }

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public object? Data { get; set; }

}