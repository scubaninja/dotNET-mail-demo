using System.Dynamic;
using Markdig;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Tailwind.Mail.Services;

namespace Tailwind.Mail.Models;

/// <summary>
/// Represents an email constructed from a markdown document with YAML frontmatter.
/// 
/// This class parses markdown content with frontmatter metadata (Subject, Summary, Lang, etc.)
/// and generates both raw HTML and accessible HTML with WCAG 2.1 compliant structure.
/// 
/// Example markdown format:
/// <code>
/// ---
/// Subject: "Welcome Email"
/// Summary: "Thank you for signing up"
/// Lang: "en"
/// SendToTag: "*"
/// ---
/// 
/// # Welcome!
/// Your email content here...
/// </code>
/// </summary>
public class MarkdownEmail{
  /// <summary>
  /// The raw markdown content including YAML frontmatter.
  /// </summary>
  public string? Markdown { get; set; }
  
  /// <summary>
  /// The rendered HTML from markdown content (without accessibility wrapper).
  /// </summary>
  public string? Html { get; set; }
  
  /// <summary>
  /// Gets the fully accessible HTML email with proper semantic structure, 
  /// ARIA attributes, live regions for screen reader support, skip links,
  /// and high contrast/reduced motion CSS support.
  /// This is the recommended output for sending emails.
  /// </summary>
  public string? AccessibleHtml { get; set; }
  
  /// <summary>
  /// Dynamic object containing parsed YAML frontmatter data.
  /// Common properties: Subject, Summary, Slug, Lang, SendToTag, Prompt
  /// </summary>
  public dynamic? Data { get; set; }
  
  /// <summary>
  /// List of tags associated with this email for filtering/categorization.
  /// </summary>
  public List<string> Tags { get; set; } = new List<string>();
  
  /// <summary>
  /// When true (default), wraps the HTML content in an accessible template
  /// with ARIA roles, live regions, and semantic structure.
  /// Set to false to use raw HTML without accessibility enhancements.
  /// </summary>
  public bool UseAccessibleTemplate { get; set; } = true;
  
  /// <summary>
  /// Default constructor for MarkdownEmail.
  /// Use FromFile() or FromString() factory methods to create instances.
  /// </summary>
  public MarkdownEmail()
  {

  }
  /// <summary>
  /// Creates a MarkdownEmail from a file path.
  /// Reads the file content and renders it to HTML.
  /// </summary>
  /// <param name="path">Path to the markdown file with YAML frontmatter</param>
  /// <returns>A new MarkdownEmail instance with rendered content</returns>
  public static MarkdownEmail FromFile(string path){
    var email = new MarkdownEmail();
    email.Markdown = File.ReadAllText(path);
    email.Render();
    return email;
  }
  
  /// <summary>
  /// Creates a MarkdownEmail from a markdown string.
  /// </summary>
  /// <param name="markdown">Markdown content with YAML frontmatter</param>
  /// <returns>A new MarkdownEmail instance with rendered content</returns>
  public static MarkdownEmail FromString(string markdown){
    var email = new MarkdownEmail();
    email.Markdown = markdown;
    email.Render();
    return email;
  }
  
  /// <summary>
  /// Validates that required frontmatter fields are present.
  /// Required fields: Subject, Summary
  /// </summary>
  /// <returns>True if the email has valid Subject and Summary</returns>
  public bool IsValid(){
    if(Data == null) return false;
    return Data.Subject != null && Data.Summary != null;
  }
  
  /// <summary>
  /// Renders the markdown content to HTML and generates accessible HTML.
  /// This method:
  /// 1. Converts markdown to HTML using Markdig with advanced extensions
  /// 2. Parses YAML frontmatter to extract metadata (Subject, Summary, Lang, etc.)
  /// 3. Auto-generates Slug from Subject if not provided
  /// 4. Sets default SendToTag to "*" (all contacts) if not specified
  /// 5. Wraps content in accessible HTML template with ARIA attributes
  /// </summary>
  private void Render(){
    // Validate markdown content exists
    if(Markdown == null){
      throw new Exception("Markdown is null; be sure to set that first");
    }
    
    // Step 1: Convert markdown to HTML using Markdig with advanced extensions
    // Advanced extensions include: tables, task lists, auto-links, etc.
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    Html = Markdig.Markdown.ToHtml(Markdown, pipeline);

    // Step 2: Parse YAML frontmatter using YamlDotNet
    // The frontmatter is between --- delimiters at the start of the document
    var yamler = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    using (var input = new StringReader(Markdown))
    {
      var parser = new Parser(input);
      parser.Consume<StreamStart>();
      parser.Consume<DocumentStart>();
      // Deserialize YAML to ExpandoObject for dynamic property access
      Data = yamler.Deserialize<ExpandoObject>(parser);
      parser.Consume<DocumentEnd>();
    }
    
    // Step 3: Set default values for optional frontmatter fields
    var expando = (IDictionary<string, object>)Data;
    
    // Auto-generate slug from subject if not provided (URL-safe format)
    if(!expando.ContainsKey("Slug")){
      Data.Slug = Data.Subject.ToLower().Replace(" ", "-");
    }
    
    // Default to sending to all contacts if no tag filter specified
    if(!expando.ContainsKey("SendToTag")){
      Data.SendToTag = "*"; // "*" means send to everyone
    }
    
    // Step 4: Generate accessible HTML with screen reader support and live regions
    // This wraps the content with semantic structure, ARIA roles, skip links, etc.
    if(UseAccessibleTemplate && Html != null && Data != null){
      var subject = Data.Subject?.ToString() ?? "";
      var summary = Data.Summary?.ToString() ?? "";
      // Get language from frontmatter or default to "en"
      var lang = expando.ContainsKey("Lang") ? Data.Lang?.ToString() ?? "en" : "en";
      AccessibleHtml = AccessibleEmailTemplate.Wrap(subject, summary, Html, lang);
    } else {
      AccessibleHtml = Html;
    }
  }
}