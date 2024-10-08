using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

/// <summary>
/// De klasse Broadcast vertegenwoordigt een e-mailuitzending.
/// </summary>
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  /// <summary>
  /// De unieke ID van de uitzending.
  /// </summary>
  public int? ID { get; set; }

  /// <summary>
  /// De ID van de e-mail die wordt uitgezonden.
  /// </summary>
  public int? EmailId { get; set; }

  /// <summary>
  /// De status van de uitzending. Standaard is "pending".
  /// </summary>
  public string Status { get; set; } = "pending";

  /// <summary>
  /// De naam van de uitzending.
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  /// De slug van de uitzending.
  /// </summary>
  public string? Slug { get; set; }

  /// <summary>
  /// Het e-mailadres waarnaar antwoorden moeten worden gestuurd.
  /// </summary>
  public string? ReplyTo { get; set; }

  /// <summary>
  /// De tag waaraan de uitzending wordt verzonden. Standaard is "*".
  /// </summary>
  public string SendToTag { get; set; } = "*";

  /// <summary>
  /// De datum en tijd waarop de uitzending is aangemaakt. Standaard is de huidige UTC-tijd.
  /// </summary>
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  /// <summary>
  /// Privé constructor om te voorkomen dat een Broadcast object direct wordt aangemaakt.
  /// </summary>
  private Broadcast()
  {
    
  }

  /// <summary>
  /// Maakt een Broadcast object op basis van een MarkdownEmail object.
  /// </summary>
  /// <param name="doc">Het MarkdownEmail document.</param>
  /// <returns>Een nieuw Broadcast object.</returns>
  public static Broadcast FromMarkdownEmail(MarkdownEmail doc){
    var broadcast = new Broadcast();
    broadcast.Name = doc.Data.Subject;
    broadcast.Slug = doc.Data.Slug;
    broadcast.SendToTag = doc.Data.SendToTag;
    return broadcast;
  }

  /// <summary>
  /// Maakt een Broadcast object op basis van een markdown string.
  /// </summary>
  /// <param name="markdown">De markdown string.</param>
  /// <returns>Een nieuw Broadcast object.</returns>
  public static Broadcast FromMarkdown(string markdown){
    var broadcast = new Broadcast();
    var doc = MarkdownEmail.FromString(markdown);
    broadcast.Name = doc.Data.Subject;
    broadcast.Slug = doc.Data.Slug;
    broadcast.SendToTag = doc.Data.SendToTag;
    return broadcast;
  }

  /// <summary>
  /// Berekent het aantal contacten dat de uitzending zal ontvangen.
  /// </summary>
  /// <param name="conn">De databaseverbinding.</param>
  /// <returns>Het aantal contacten.</returns>
  public long ContactCount(IDbConnection conn){
    // Hebben we een tag?
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