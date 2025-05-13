using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;


[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  public int? ID { get; set; }
  public int? EmailId { get; set; }
  public string Status { get; set; } = "pending";
  public string? Name { get; set; }
  public string? Slug { get; set; }
  public string? ReplyTo { get; set; }
  public string SendToTag { get; set; } = "*";
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  private Broadcast() { }

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

  public static Broadcast FromMarkdown(string markdown) {
    if (string.IsNullOrWhiteSpace(markdown)) {
      throw new ArgumentException("Markdown content cannot be null or empty", nameof(markdown));
    }

    var doc = MarkdownEmail.FromString(markdown);
    return FromMarkdownEmail(doc);
  }

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