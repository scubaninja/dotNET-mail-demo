using System.Dynamic;
using Markdig;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Tailwind.Mail.Services;

namespace Tailwind.Mail.Models;

public class MarkdownEmail{
  public string? Markdown { get; set; }
  public string? Html { get; set; }
  /// <summary>
  /// Gets the fully accessible HTML email with proper structure, ARIA attributes,
  /// and live regions for screen reader support.
  /// </summary>
  public string? AccessibleHtml { get; set; }
  public dynamic? Data { get; set; }
  public List<string> Tags { get; set; } = new List<string>();
  /// <summary>
  /// When true, uses the accessible HTML wrapper template.
  /// Set to false to use raw HTML without accessibility enhancements.
  /// </summary>
  public bool UseAccessibleTemplate { get; set; } = true;
  public MarkdownEmail()
  {

  }
  public static MarkdownEmail FromFile(string path){
    var email = new MarkdownEmail();
    email.Markdown = File.ReadAllText(path);
    email.Render();
    return email;
  } 
  public static MarkdownEmail FromString(string markdown){
    var email = new MarkdownEmail();
    email.Markdown = markdown;
    email.Render();
    return email;
  }
  public bool IsValid(){
    if(Data == null) return false;
    return Data.Subject != null && Data.Summary != null;
  }
  private void Render(){
    if(Markdown == null){
      throw new Exception("Markdown is null; be sure to set that first");
    }
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    Html = Markdig.Markdown.ToHtml(Markdown, pipeline);

    //data
    var yamler = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    using (var input = new StringReader(Markdown))
    {
      var parser = new Parser(input);
      parser.Consume<StreamStart>();
      parser.Consume<DocumentStart>();
      //dyamic acti0n
      Data = yamler.Deserialize<ExpandoObject>(parser);
      parser.Consume<DocumentEnd>();
    }
    var expando = (IDictionary<string, object>)Data;
    if(!expando.ContainsKey("Slug")){
      Data.Slug = Data.Subject.ToLower().Replace(" ", "-");
    }
    if(!expando.ContainsKey("SendToTag")){
      Data.SendToTag = "*"; //send to everyone
    }
    
    // Generate accessible HTML with screen reader support and live regions
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