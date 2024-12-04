using Tailwind.Data;
using Dapper;
using System.Data;

namespace Tailwind.Mail.Models;

/// <summary>
/// Represents a broadcast message to be sent to contacts.
/// </summary>
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  /// <summary>
  /// Gets or sets the ID of the broadcast.
  /// </summary>
  public int? ID { get; set; }

  /// <summary>
  /// Gets or sets the ID of the associated email.
  /// </summary>
  public int? EmailId { get; set; }

  /// <summary>
  /// Gets or sets the status of the broadcast.
  /// </summary>
  public string Status { get; set; } = "pending";

  /// <summary>
  /// Gets or sets the name of the broadcast.
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  /// Gets or sets the slug of the broadcast.
  /// </summary>
  public string? Slug { get; set; }

  /// <summary>
  /// Gets or sets the reply-to address for the broadcast.
  /// </summary>
  public string? ReplyTo { get; set; }

  /// <summary>
  /// Gets or sets the tag to which the broadcast will be sent.
  /// </summary>
  public string SendToTag { get; set; } = "*";

  /// <summary>
  /// Gets or sets the creation date and time of the broadcast.
  /// </summary>
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  /// <summary>
  /// Initializes a new instance of the <see cref="Broadcast"/> class.
  /// </summary>
  private Broadcast() { }

  /// <summary>
  /// Creates a new <see cref="Broadcast"/> instance from a <see cref="MarkdownEmail"/> document.
  /// </summary>
  /// <param name="doc">The markdown email document.</param>
  /// <returns>A new <see cref="Broadcast"/> instance.</returns>
  public static Broadcast FromMarkdownEmail(MarkdownEmail doc) {
    var broadcast = new Broadcast {
      Name = doc.Data.Subject,
      Slug = doc.Data.Slug,
      SendToTag = doc.Data.SendToTag
    };
    return broadcast;
  }

  /// <summary>
  /// Creates a new <see cref="Broadcast"/> instance from a markdown string.
  /// </summary>
  /// <param name="markdown">The markdown string.</param>
  /// <returns>A new <see cref="Broadcast"/> instance.</returns>
  public static Broadcast FromMarkdown(string markdown) {
    var broadcast = new Broadcast();
    var doc = MarkdownEmail.FromString(markdown);
    broadcast.Name = doc.Data.Subject;
    broadcast.Slug = doc.Data.Slug;
    broadcast.SendToTag = doc.Data.SendToTag;
    return broadcast;
  }

  /// <summary>
  /// Gets the count of contacts to which the broadcast will be sent.
  /// </summary>
  /// <param name="conn">The database connection.</param>
  /// <returns>The count of contacts.</returns>
  public long ContactCount(IDbConnection conn) {
    long contacts = 0;
    if (SendToTag == "*") {
      contacts = conn.ExecuteScalar<long>("select count(1) from mail.contacts where subscribed = true");
    } else {
      // Additional logic for counting contacts by tag
    }
    return contacts;
  }
}