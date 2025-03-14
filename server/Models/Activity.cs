using Dapper;
using Tailwind.Data;

namespace Tailwind.Mail.Models;

/// Represents an activity record for tracking contact interactions and events.
/// Activities are used to log various actions and events related to contacts
/// in the mailing system.
[Table("activity", Schema = "mail")]
public class Activity {
    /// Unique identifier for the activity record
    public int? ID { get; set; }

    /// Reference to the associated contact
    public int? ContactId { get; set; }


    /// Unique key for the activity, automatically generated as a GUID
    public string Key { get; set; } = Guid.NewGuid().ToString();


