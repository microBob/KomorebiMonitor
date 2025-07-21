using System.Text.Json.Serialization;

namespace KomorebiMonitor.models;

public partial class NotificationMessage
{
    [JsonPropertyName("event")]
    public NotificationEvent Event { get; set; }

    [JsonPropertyName("state")]
    public State State { get; set; }
}
