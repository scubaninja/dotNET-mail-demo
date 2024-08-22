using System.Dynamic;
using Markdig;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Tailwind.Mail.Models;

public class MarkdownEmail{
  public string? Markdown { get; set; }
  public string? Html { get; set; }
  public dynamic? Data { get; set; }
  public List<string> Tags { get; set; } = new List<string>();
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
  }
}