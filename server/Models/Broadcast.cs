using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

/// <summary>
/// Represents an email broadcast that can be sent to multiple recipients.
/// A broadcast is created from markdown content and can target specific segments using tags.
/// The creation process follows these steps:
/// 1. Create initial data (name, slug)
/// 2. Add email content 
/// 3. Define segment to send to using tags
/// </summary>
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
    // Unique identifier for the broadcast
    public int? ID { get; set; }

    // Reference to the associated email template
    public int? EmailId { get; set; }

    // Current status of the broadcast (pending, sent, etc)
    public string Status { get; set; } = "pending";

    // Display name for the broadcast
    public string? Name { get; set; }

    // URL-friendly identifier for the broadcast
    public string? Slug { get; set; }

    // Optional reply-to email address
    public string? ReplyTo { get; set; }

    // Tag used to segment recipients. "*" means send to all subscribers
    public string SendToTag { get; set; } = "*";

    // Timestamp when broadcast was created
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Private constructor prevents direct instantiation
    private Broadcast() { }

    /// <summary>
    /// Creates a new broadcast from a parsed markdown email document
    /// </summary>
    /// <param name="doc">The parsed markdown email containing metadata and content</param>
    /// <returns>A new Broadcast instance</returns>
    /// <exception cref="ArgumentNullException">Thrown if doc or doc.Data is null</exception>
    public static Broadcast FromMarkdownEmail(MarkdownEmail doc) {
        if (doc == null || doc.Data == null) {
            throw new ArgumentNullException(nameof(doc), "MarkdownEmail document cannot be null");
        }

        // Map markdown frontmatter to broadcast properties
        var broadcast = new Broadcast {
            Name = doc.Data.Subject,
            Slug = doc.Data.Slug,
            SendToTag = doc.Data.SendToTag
        };

        return broadcast;
    }

    /// <summary>
    /// Creates a new broadcast from raw markdown content
    /// </summary>
    /// <param name="markdown">Raw markdown string containing frontmatter and content</param>
    /// <returns>A new Broadcast instance</returns>
    /// <exception cref="ArgumentException">Thrown if markdown is null or empty</exception>
    public static Broadcast FromMarkdown(string markdown) {
        if (string.IsNullOrWhiteSpace(markdown)) {
            throw new ArgumentException("Markdown content cannot be null or empty", nameof(markdown));
        }

        // Parse markdown into document and create broadcast
        var doc = MarkdownEmail.FromString(markdown);
        return FromMarkdownEmail(doc);
    }

    /// Counts how many contacts will receive this broadcast based on the SendToTag
    /// <param name="conn">Database connection to use for the query</param>
    /// <returns>Number of contacts that match the broadcast's targeting criteria</returns>
    /// <exception cref="ArgumentNullException">Thrown if conn is null</exception>
    /// <exception cref="ApplicationException">Thrown if database query fails</exception>
    public long ContactCount(IDbConnection conn) {
        if (conn == null) {
            throw new ArgumentNullException(nameof(conn), "Database connection cannot be null");
        }

        long contacts = 0;
        try {
            // For broadcasts targeting all subscribers
            if (SendToTag == "*") {
                contacts = conn.ExecuteScalar<long>("SELECT COUNT(1) FROM mail.contacts WHERE subscribed = true");
            }
            // For broadcasts targeting a specific tag
            else {
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
            // Wrap DB exceptions in application exception
            throw new ApplicationException("An error occurred while counting contacts", ex);
        }

        return contacts;
    }
}



    /// <summary>
    /// Creates a new broadcast from a parsed markdown email document.
    /// This factory method handles the initialization of a broadcast instance
    /// from pre-parsed markdown content.
    /// </summary>
    /// <param name="doc">The parsed markdown email containing metadata and content.
    /// Expected to have Subject, Slug, and SendToTag in its Data property.</param>
    /// <returns>A new configured Broadcast instance with properties set from the markdown data</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when:
    /// - doc is null
    /// - doc.Data is null
    /// </exception>
    /// <remarks>
    /// The method maps the following properties:
    /// - doc.Data.Subject → Broadcast.Name
    /// - doc.Data.Slug → Broadcast.Slug
    /// - doc.Data.SendToTag → Broadcast.SendToTag
    /// </remarks>
    public static Broadcast FromMarkdownEmail(MarkdownEmail doc)

    /// <summary>
    /// Creates a new broadcast from raw markdown content.
    /// This is a convenience method that parses the markdown string
    /// and delegates to FromMarkdownEmail for broadcast creation.
    /// </summary>
    /// <param name="markdown">Raw markdown string containing frontmatter and content.
    /// The frontmatter should include subject, slug, and sendToTag fields.</param>
    /// <returns>A new Broadcast instance configured from the markdown content</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the markdown parameter is null, empty, or consists only of whitespace
    /// </exception>
    /// <remarks>
    /// The method:
    /// 1. Validates the input markdown
    /// 2. Parses it into a MarkdownEmail document
    /// 3. Delegates to FromMarkdownEmail for broadcast creation
    /// </remarks>
    public static Broadcast FromMarkdown(string markdown)

    /// <summary>
    /// Counts the number of contacts that will receive this broadcast based on the SendToTag criteria.
    /// Handles both all-subscriber broadcasts (SendToTag = "*") and tag-specific broadcasts.
    /// </summary>
    /// <param name="conn">Active database connection to use for the query</param>
    /// <returns>
    /// The number of subscribed contacts that match the broadcast's targeting criteria.
    /// Returns 0 if no matching contacts are found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the database connection parameter is null
    /// </exception>
    /// <exception cref="ApplicationException">
    /// Thrown when database operations fail, wrapping the original exception
    /// </exception>
    /// <remarks>
    /// For SendToTag = "*": Counts all subscribed contacts
    /// For specific tags: Counts subscribed contacts with the specified tag
    /// Uses Dapper's ExecuteScalar for efficient counting
    /// </remarks>
    public long ContactCount(IDbConnection conn)