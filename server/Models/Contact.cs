
using Dapper;
using System.Text.Json.Serialization;
namespace Tailwind.Mail.Models;

public class SignUpRequest
{
  public string Name { get; set; }
  public string Email { get; set; }
  public bool ConsentToProcessing { get; set; }
  public string ConsentSource { get; set; } = "Web Form";
}

[Table("contacts", Schema = "mail")]
public class Contact
{
  public string Name { get; set; }
  public string Email { get; set; }
  
  /// <summary>
  /// Indicates whether the user has consented to receive communications
  /// </summary>
  public bool Subscribed { get; set; }
  
  /// <summary>
  /// Unique identifier for this contact, used for subscription management
  /// </summary>
  public string Key { get; set; } = Guid.NewGuid().ToString();
  
  public int? ID { get; set; }
  
  /// <summary>
  /// When this contact was created
  /// </summary>
  public DateTimeOffset CreatedAt { get; set; }
  
  /// <summary>
  /// When this contact explicitly consented to data processing
  /// </summary>
  public DateTimeOffset? ConsentTimestamp { get; set; }
  
  /// <summary>
  /// Version of privacy policy that was consented to
  /// </summary>
  public string PrivacyPolicyVersion { get; set; } = "1.0";
  
  /// <summary>
  /// Source of the consent (e.g., "Web Form", "Import", "API")
  /// </summary>
  public string ConsentSource { get; set; } = "Web Form";
  
  /// <summary>
  /// IP address where consent was given - stored for compliance/audit purposes
  /// </summary>
  public string ConsentIPAddress { get; set; }
  
  /// <summary>
  /// When this contact's data should be anonymized according to retention policy
  /// </summary>
  public DateTimeOffset? DataRetentionExpiry { get; set; }
  
  /// <summary>
  /// Indicates if this contact has requested data deletion
  /// </summary>
  [JsonIgnore] // Don't include in API responses
  public bool DeletionRequested { get; set; } = false;
  
  public Contact()
  {
    
  }
  
  public Contact(string name, string email)
  {
    Name = name;
    Email = email;
  }

}