using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

// The process of creating a broadcast is:
// The initial data is created first (name, slug)
// Then the email and finally the segment to send to, which is done by tag (or not)
// If the initial use case is using a markdown document, then it should contain all 
// that we need
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  // Primary key for the broadcast
  public int? ID { get; set; }
  
  // Foreign key to the email associated with this broadcast
  public int? EmailId { get; set; }
  
  // Status of the broadcast, default is "pending"
  public string Status { get; set; } = "pending";
  
  // Name of the broadcast
  public string? Name { get; set; }
  
  // URL-friendly slug for the broadcast
  public string? Slug { get; set; }
  
  // Reply-to email address for the broadcast
  public string? ReplyTo { get; set; }
  
  // Tag to segment the recipients, default is "*" which means all
  public string SendToTag { get; set; } = "*";
  
  // Timestamp when the broadcast was created
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  
  // Private constructor to prevent direct instantiation
  private Broadcast()
  {
    
  }
  
  // Factory method to create a Broadcast instance from a MarkdownEmail document
  public static Broadcast FromMarkdownEmail(MarkdownEmail doc){
    var broadcast = new Broadcast();
    broadcast.Name = doc.Data.Subject;
    broadcast.Slug = doc.Data.Slug;
    broadcast.SendToTag = doc.Data.SendToTag;
    return broadcast;
  }
  
  // Method to count the number of contacts to send the broadcast to
  public long ContactCount(IDbConnection conn){
    // Initialize contact count
    long contacts = 0;
    
    // Check if the broadcast is to be sent to all contacts
    if(SendToTag == "*"){
      // Count all subscribed contacts
      contacts = conn.ExecuteScalar<long>("select count(1) from mail.contacts where subscribed = @subscribed", new { subscribed = true });
    }else{
      // SQL query to count contacts with the specified tag
      var sql = @"
        select count(1) as count from mail.contacts 
        inner join mail.tagged on mail.tagged.contact_id = mail.contacts.id
        inner join mail.tags on mail.tags.id = mail.tagged.tag_id
        where subscribed = @subscribed
        and tags.slug = @tagSlug
      ";
      // Execute the query with the tag slug parameter
      contacts = conn.ExecuteScalar<long>(sql, new { subscribed = true, tagSlug = SendToTag });
    }
    
    // Return the contact count
    return contacts;
  }
}