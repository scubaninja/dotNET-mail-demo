using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

/// <summary>
/// Represents an email broadcast campaign that can be sent to multiple recipients.
/// The broadcast creation process follows these steps:
/// 1. Create initial data (name, slug)
/// 2. Add email content
/// 3. Define segment by tag (optional)
/// </summary>
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
    /// <summary>
    /// The unique identifier for the broadcast
    /// </summary>
    public int? ID { get; set; }

    /// <summary>
    /// Reference to the associated email content
    /// </summary>
    public int? EmailId { get; set; }

    /// <summary>
    /// Current status of the broadcast (pending, sent, failed, etc.)
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Human-readable name of the broadcast campaign
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// URL-friendly version of the name
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Email address that recipients can reply to
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Tag used to segment recipients. "*" means send to all.
    /// </summary>
    public string SendToTag { get; set; } = "*";

    /// <summary>
    /// Timestamp when the broadcast was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Private constructor to enforce use of factory methods
    /// </summary>
    private Broadcast()
    {
    }

    /// <summary>
    /// Creates a new broadcast from a markdown email document
    /// </summary>
    /// <param name="doc">The markdown email document containing broadcast content</param>
    /// <returns>A new broadcast instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when doc is null</exception>
    public static Broadcast FromMarkdownEmail(MarkdownEmail doc){
        var broadcast = new Broadcast();
        broadcast.Name = doc.Data.Subject;
        broadcast.Slug = doc.Data.Slug;
        broadcast.SendToTag = doc.Data.SendToTag;
        return broadcast;
    }

    public static Broadcast FromMarkdown(string markdown){
        var broadcast = new Broadcast();
        var doc = MarkdownEmail.FromString(markdown);
        broadcast.Name = doc.Data.Subject;
        broadcast.Slug = doc.Data.Slug;
        broadcast.SendToTag = doc.Data.SendToTag;
        return broadcast;
    }

    public long ContactCount(IDbConnection conn){
        //do we have a tag?
        long contacts = 0;
        if(SendToTag == "*"){
            contacts = conn.ExecuteScalar<long>("select count(1) from mail.contacts where subscribed = true");
        }else{
            var sql = @"
                select count(1) as count from mail.contacts 
                inner join mail.tagged on mail.tagged.contact_id = mail.contacts.id
                inner join mail.tags on mail.tags.id = mail.tagged.tag_id
                where subscribed = true
                and tags.slug = @tagSlug
            ";
            contacts = conn.ExecuteScalar<long>(sql, new {tagSlug = SendToTag});
        }
        return contacts;
    }
}