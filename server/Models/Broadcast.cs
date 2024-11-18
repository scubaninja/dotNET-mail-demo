using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

/// <summary>
/// The process of creating a broadcast is:
/// 1. The initial data is created first (name, slug)
/// 2. Then the email is created
/// 3. Finally, the segment to send to is created, which is done by tag (or not)
/// If the initial use case is using a markdown document, then it should contain all that we need.
/// </summary>
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  /// <summary>
  /// Gets or sets the unique identifier for the broadcast.
  /// </summary>
  public int? ID { get; set; }

  /// <summary>
  /// Gets or sets the unique identifier for the associated email.
  /// </summary>
  public int? EmailId { get; set; }

  /// <summary>
  /// Gets or sets the status of the broadcast. Default is "pending".
  /// </summary>
  public string Status { get; set; } = "pending";

  /// <summary>
  /// Gets or sets the name of the broadcast.
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  /// Gets or sets the slug (URL-friendly version of the name) of the broadcast.
  /// </summary>
  public string? Slug { get; set; }

  /// <summary>
  /// Gets or sets the reply-to address for the broadcast.
  /// </summary>
  public string? ReplyTo { get; set; }

  /// <summary>
  /// Gets or sets the tag to send the broadcast to. Default is "*".
  /// </summary>
  public string SendToTag { get; set; } = "*";
}