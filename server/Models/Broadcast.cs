using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

//The process of creating a broadcast is:
//The initial data is created first (name, slug)
//The the email and finally the segment to send to, which is done by tag (or not)
//If the initial use case is using a markdown document, then it should contain all 
//that we need
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  public int? ID { get; set; }
  public int? EmailId { get; set; }
  public string Status { get; set; } = "pending";
  public string? Name { get; set; }
  public string? Slug { get; set; }
  public string? ReplyTo { get; set; }
  public string SendToTag { get; set; } = "*";
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  private Broadcast()
  {
    
  }
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