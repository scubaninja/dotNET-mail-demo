using Markdig;
using Dapper;
namespace Tailwind.Mail.Models;

/// <summary>
/// Represents an email message in the system.
/// Maps to the 'emails' table in the 'mail' schema.
/// </summary>
[Table("emails", Schema = "mail")]
public class Email{
    /// <summary>
    /// Gets or sets the unique identifier for the email.
    /// </summary>
    public int? ID { get; set; }

    /// <summary>
    /// Gets or sets the URL-friendly identifier for the email.
    /// </summary>
    public string Slug { get; set; }

    /// <summary>
    /// Gets or sets the email subject line.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the preview text shown in email clients.
    /// </summary>
    public string Preview { get; set; }

    /// <summary>
    /// Gets or sets the delay in hours before sending the email.
    /// Default is 0 (immediate sending).
    /// </summary>
    public int DelayHours { get; set; } = 0;

    /// <summary>
    /// Gets or sets the HTML content of the email.
    /// </summary>
    public string Html { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Initializes a new instance of the Email class from a MarkdownEmail document.
    /// </summary>
    /// <param name="doc">The MarkdownEmail document containing email content and metadata.</param>
    /// <exception cref="InvalidDataException">Thrown when required markdown metadata or HTML content is missing.</exception>
    public Email(MarkdownEmail doc)
    {
        if (doc.Data == null)
        {
            throw new InvalidDataException("Markdown document should contain Slug, Subject, and Summary at least");
        }
        if (doc.Html == null)
        {
            throw new InvalidDataException("There should be HTML generated from the markdown document");
        }
        Slug = doc.Data.Slug;
        Subject = doc.Data.Subject;
        Preview = doc.Data.Summary;
        Html = doc.Html;
    }
}