using System.Text.Json.Serialization;

namespace snapnow.DTOS;

public class UserInfoGoogleModel
{
    [JsonPropertyName("resourceName")]
    public string ResourceName { get; set; }
    
    [JsonPropertyName("emailAddresses")]
    public List<EmailAddressesGoogleResponse> EmailAddresses { get; set; }
}