using Markdig;
using Dapper;
namespace Tailwind.Mail.Models;

/// <summary>
/// Represents an email template that can be used to send messages.
/// Contains the email content and metadata needed for sending.
/// </summary>
[Table("emails", Schema = "mail")]
public class Email {
    /// <summary>
    /// Unique identifier for the email template
    /// </summary>
    public int? ID { get; set; }

    /// <summary>
    /// URL-friendly identifier for the email template
    /// </summary>
    public string Slug { get; set; }

    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Short preview text shown in email clients
    /// </summary>
    public string Preview { get; set; }

    /// <summary>
    /// Number of hours to delay sending after creation
    /// </summary>
    public int DelayHours { get; set; } = 0;

    /// <summary>
    /// HTML content of the email
    /// </summary>
    public string Html { get; set; }

    /// <summary>
    /// Timestamp when the email template was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// Creates a new email template from a markdown document
    /// <param name="doc">Parsed markdown document containing email content and metadata</param>
    /// <exception cref="InvalidDataException">
    /// Thrown when:
    /// - doc.Data is null or missing required fields (Slug, Subject, Summary)
    /// - doc.Html is null (markdown wasn't properly converted to HTML)
    /// </exception>
    /// <remarks>
    /// The markdown document must contain frontmatter with:
    /// - slug: URL-friendly identifier
    /// - subject: Email subject line
    /// - summary: Preview text
    /// The main content will be converted to HTML
    /// </remarks>
    public Email(MarkdownEmail doc)
    {
        if(doc.Data == null){
            throw new InvalidDataException("Markdown document should contain Slug, Subject, and Summary at least");
        }
        if(doc.Html == null){
            throw new InvalidDataException("There should be HTML generated from the markdown document");
        }
        Slug = doc.Data.Slug;
        Subject = doc.Data.Subject;
        Preview = doc.Data.Summary;
        Html = doc.Html;
    }
}