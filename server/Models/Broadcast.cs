using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

/// <summary>
/// Represents a broadcast email campaign, including metadata and methods for processing.
/// </summary>
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  /// <summary>
  /// Gets or sets the unique identifier of the broadcast.
  /// </summary>
  public int? ID { get; set; }

  /// <summary>
  /// Gets or sets the associated email ID for the broadcast.
  /// </summary>
  public int? EmailId { get; set; }

  /// <summary>
  /// Gets or sets the status of the broadcast (e.g., "pending", "sent").
  /// </summary>
  public string Status { get; set; } = "pending";

  /// <summary>
  /// Gets or sets the name of the broadcast.
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  /// Gets or sets the slug (URL-friendly identifier) for the broadcast.
  /// </summary>
  public string? Slug { get; set; }

  /// <summary>
  /// Gets or sets the reply-to email address for the broadcast.
  /// </summary>
  public string? ReplyTo { get; set; }

  /// <summary>
  /// Gets or sets the tag used to filter the audience for the broadcast.
  /// </summary>
  public string SendToTag { get; set; } = "*";

  /// <summary>
  /// Gets or sets the timestamp when the broadcast was created.
  /// </summary>
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  private Broadcast() { }

  /// <summary>
  /// Creates a new <see cref="Broadcast"/> instance from a <see cref="MarkdownEmail"/> object.
  /// </summary>
  /// <param name="doc">The markdown email document containing broadcast data.</param>
  /// <returns>A new <see cref="Broadcast"/> instance.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the provided document is null.</exception>
  public static Broadcast FromMarkdownEmail(MarkdownEmail doc) {
    if (doc == null || doc.Data == null) {
      throw new ArgumentNullException(nameof(doc), "MarkdownEmail document cannot be null");
    }

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
  /// <param name="markdown">The markdown content containing broadcast data.</param>
  /// <returns>A new <see cref="Broadcast"/> instance.</returns>
  /// <exception cref="ArgumentException">Thrown if the markdown content is null or empty.</exception>
  public static Broadcast FromMarkdown(string markdown) {
    if (string.IsNullOrWhiteSpace(markdown)) {
      throw new ArgumentException("Markdown content cannot be null or empty", nameof(markdown));
    }

    var doc = MarkdownEmail.FromString(markdown);
    return FromMarkdownEmail(doc);
  }

  /// <summary>
  /// Calculates the number of contacts that match the broadcast's audience criteria.
  /// </summary>
  /// <param name="conn">The database connection to use for the query.</param>
  /// <returns>The number of matching contacts.</returns>
  /// <exception cref="ArgumentNullException">Thrown if the database connection is null.</exception>
  /// <exception cref="ApplicationException">Thrown if an error occurs during the query.</exception>
  public long ContactCount(IDbConnection conn) {
    if (conn == null) {
      throw new ArgumentNullException(nameof(conn), "Database connection cannot be null");
    }

    long contacts = 0;
    try {
      if (SendToTag == "*") {
        contacts = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM mail.contacts WHERE subscribed = true");
      } else {
        var sql = @"
          SELECT COUNT(1) AS count FROM mail.contacts 
          INNER JOIN mail.tagged ON mail.tagged.contact_id = mail.contacts.id
          INNER JOIN mail.tags ON mail.tags.id = mail.tagged.tag_id
          WHERE subscribed = true
          AND tags.slug = @tagSlug
        ";
        contacts = conn.ExecuteScalar<long>(sql, new { tagSlug = SendToTag });
      }
    } catch (Exception ex) {
      // Log the exception (logging mechanism not shown here)
      throw new ApplicationException("An error occurred while counting contacts", ex);
    }

    return contacts;
  }
}