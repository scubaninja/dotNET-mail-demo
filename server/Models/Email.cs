using Markdig;
using Dapper;
namespace Tailwind.Mail.Models;

[Table("emails", Schema = "mail")]
public class Email{
  public int? ID { get; set; }
  public string Slug { get; set; }
  public string Subject { get; set; }
  public string Preview { get; set; }
  public int DelayHours { get; set; }=0;
  public string Html { get; set; }
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  public Email(MarkdownEmail doc)
  {
    //check this again
    if(doc.Data == null){
      throw new InvalidDataException("Markdown document should contain Slug, Subject, and Summary at least");
    }
    if(doc.Html == null){
      throw new InvalidDataException("There should be HTML generated from the markdown document");
    }
    Slug = doc.Data.Slug;
    Subject = doc.Data.Subject;
    Preview = doc.Data.Summary;
    // Use accessible HTML template with screen reader support and ARIA attributes
    Html = doc.AccessibleHtml ?? doc.Html;
  }
}