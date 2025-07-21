using System.Text.Json.Serialization;

namespace KomorebiMonitor.models;

public partial class SocketMessage
{
    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    [JsonPropertyName("type")]
    public TypeEnum Type { get; set; }
}