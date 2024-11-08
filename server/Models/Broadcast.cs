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
  // Primary key for the broadcast
  public int? ID { get; set; }
  
  // Foreign key to the email associated with the broadcast
  public int? EmailId { get; set; }
  
  // Status of the broadcast, default is "pending"
  public string Status { get; set; } = "pending";
  
  // Name of the broadcast
  public string? Name { get; set; }
  
  // Slug for the broadcast
  public string? Slug { get; set; }
  
  // Reply-to email address for the broadcast
  public string? ReplyTo { get; set; }
  
  // Tag to determine the segment of contacts to send the broadcast to, default is "*"
  public string SendToTag { get; set; } = "*";
  
  // Timestamp when the broadcast was created, default is the current UTC time
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  
  // Private constructor to prevent direct instantiation
  private Broadcast() { }
  
  // Static method to create a Broadcast instance from a MarkdownEmail object
  public static Broadcast FromMarkdownEmail(MarkdownEmail doc) {
    var broadcast = new Broadcast();
    broadcast.Name = doc.Data.Subject;
    broadcast.Slug = doc.Data.Slug;
    broadcast.SendToTag = doc.Data.SendToTag;
    return broadcast;
  }
  
  // Static method to create a Broadcast instance from a markdown string
  public static Broadcast FromMarkdown(string markdown) {
    var broadcast = new Broadcast();
    var doc = MarkdownEmail.FromString(markdown);
    broadcast.Name = doc.Data.Subject;
    broadcast.Slug = doc.Data.Slug;
    broadcast.SendToTag = doc.Data.SendToTag;
    return broadcast;
  }
  
  // Method to count the number of contacts to whom the broadcast will be sent
  public long ContactCount(IDbConnection conn) {
    long contacts = 0;
    
    // If SendToTag is "*", count all subscribed contacts
    if (SendToTag == "*") {
      contacts = conn.ExecuteScalar<long>("select count(1) from mail.contacts where subscribed = true");
    } else {
      // Otherwise, count subscribed contacts associated with the specific tag
      var sql = @"
        select count(1) as count from mail.contacts 
        inner join mail.tagged on mail.tagged.contact_id = mail.contacts.id
        inner join mail.tags on mail.tags.id = mail.tagged.tag_id
        where subscribed = true
        and tags.slug = @tagSlug
      ";
      contacts = conn.ExecuteScalar<long>(sql, new { tagSlug = SendToTag });
    }
    
    return contacts;
  }
}