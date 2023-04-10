using System.Text.Json.Serialization;

namespace snapnow.Models;

public class AccessTokenModel
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("authuser")]
    public string? AuthUser { get; set; }
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
}