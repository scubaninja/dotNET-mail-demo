using Markdig;
using Dapper;
namespace Tailwind.Mail.Models;

/// <summary>
/// Represents a persisted email record in the database.
/// 
/// This class maps to the 'mail.emails' table and stores the rendered email
/// content along with metadata. The Html property contains accessible HTML
/// with screen reader support by default.
/// 
/// Key features:
/// - Automatic accessible HTML generation from MarkdownEmail
/// - Database mapping via Dapper SimpleCRUD attributes
/// - Support for delayed sending via DelayHours
/// </summary>
[Table("emails", Schema = "mail")]
public class Email{
  /// <summary>
  /// Primary key - auto-generated database ID.
  /// </summary>
  public int? ID { get; set; }
  
  /// <summary>
  /// URL-safe identifier for the email (e.g., "welcome-email").
  /// Auto-generated from Subject if not provided in frontmatter.
  /// </summary>
  public string Slug { get; set; }
  
  /// <summary>
  /// Email subject line displayed in recipient's inbox.
  /// </summary>
  public string Subject { get; set; }
  
  /// <summary>
  /// Preview text shown in email clients (first line preview).
  /// Maps to the Summary field in markdown frontmatter.
  /// </summary>
  public string Preview { get; set; }
  
  /// <summary>
  /// Hours to delay before sending (0 = send immediately).
  /// Useful for scheduled broadcasts.
  /// </summary>
  public int DelayHours { get; set; }=0;
  
  /// <summary>
  /// The complete HTML email content with accessibility features.
  /// Includes ARIA roles, live regions, semantic structure, skip links,
  /// and high contrast/reduced motion CSS support.
  /// </summary>
  public string Html { get; set; }
  
  /// <summary>
  /// Timestamp when the email record was created.
  /// </summary>
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  
  /// <summary>
  /// Creates an Email instance from a parsed MarkdownEmail document.
  /// Extracts metadata from frontmatter and uses accessible HTML by default.
  /// </summary>
  /// <param name="doc">The parsed MarkdownEmail with rendered content</param>
  /// <exception cref="InvalidDataException">
  /// Thrown if the markdown document is missing required frontmatter (Slug, Subject, Summary)
  /// or if HTML generation failed.
  /// </exception>
  public Email(MarkdownEmail doc)
  {
    // Validate required frontmatter fields are present
    if(doc.Data == null){
      throw new InvalidDataException("Markdown document should contain Slug, Subject, and Summary at least");
    }
    
    // Ensure HTML was successfully generated from markdown
    if(doc.Html == null){
      throw new InvalidDataException("There should be HTML generated from the markdown document");
    }
    
    // Map frontmatter properties to Email fields
    Slug = doc.Data.Slug;
    Subject = doc.Data.Subject;
    Preview = doc.Data.Summary;
    
    // Use accessible HTML template with screen reader support and ARIA attributes
    // Falls back to raw HTML if accessible version is not available
    Html = doc.AccessibleHtml ?? doc.Html;
  }
}