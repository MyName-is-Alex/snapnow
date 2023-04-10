using System.Text.Json.Serialization;

namespace snapnow.DTOS;

public class EmailAddressesGoogleResponse
{
    [JsonPropertyName("value")]
    public string Value { get; set; }
}