using Xunit;
using Tailwind.Data;
using Tailwind.Mail.Commands;
using Markdig;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

[Collection("Markdown")]
public class BroadcastFromMarkdown:TestBase
{
  public string? _markdown { get; set; }
  public MarkdownEmail _doc { get; set; }
  public Broadcast _broadcast { get; set; }
  public BroadcastFromMarkdown()
  {
    //get the markdown file in the helpers directory
    //I suck at .NET... how do I get the path to the test file?
    var path = Path.Combine(Directory.GetCurrentDirectory(), "../../../Tests/Helpers", "test_broadcast.md");
    _doc = MarkdownEmail.FromFile(path);
  }
  [Fact]
  public void Given_a_markdown_file()
  {
    Assert.NotNull(_doc);
  }
  [Fact]
  public void A_valid_markdown_doc_is_produced()
  {
    Assert.NotNull(_doc.Html);
    Assert.NotNull(_doc.Data);
    Assert.Equal("Test Broadcast XX", _doc.Data.Subject);
    Assert.Equal("test-broadcast-xx", _doc.Data.Slug);
  }
  [Fact]
  public void A_valid_email_is_produced()
  {
    var email = new Email(_doc);
    Assert.NotNull(email);
    Assert.Equal("Test Broadcast XX", email.Subject);
    Assert.Equal("test-broadcast-xx", email.Slug);
    Assert.NotNull(email.Html);
  }
  [Fact]
  public void A_valid_broadcast_is_produced()
  {
    _broadcast = Broadcast.FromMarkdownEmail(_doc);
    Assert.NotNull(_broadcast);

    Assert.Equal("Test Broadcast XX", _broadcast.Name);
    Assert.Equal("test-broadcast-xx", _broadcast.Slug);
    Assert.Equal("pending", _broadcast.Status);

  }
  [Fact]
  public void A_valid_broadcast_is_saved_to_the_db()
  {
    var path = Path.Combine(Directory.GetCurrentDirectory(), "../../../Tests/Helpers", "test_broadcast.md");
    var markdown = File.ReadAllText(path);
    var mdEmail = MarkdownEmail.FromString(markdown);
    var res = new CreateBroadcast(mdEmail).Execute(Conn);
    Assert.True(res.Data.BroadcastId > 0);
    
  }
  [Fact]
  public void A_valid_tagged_broadcast_is_saved_to_the_db()
  {
    var path = Path.Combine(Directory.GetCurrentDirectory(), "../../../Tests/Helpers", "test_broadcast_tagged.md");
    var markdown = File.ReadAllText(path);
    var mdEmail = MarkdownEmail.FromString(markdown);
    var res = new CreateBroadcast(mdEmail).Execute(Conn);
    Assert.True(res.Data.BroadcastId > 0);
  }
}