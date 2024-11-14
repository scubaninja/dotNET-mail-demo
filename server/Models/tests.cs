using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

// Inqubo yokudala i-broadcast:
// Idatha yokuqala idalwa kuqala (igama, i-slug)
// Bese kuba i-imeyili kanye nesigaba okufanele sithunyelwe kuso, esenziwa ngethegi (noma cha)
// Uma icala lokuqala lisebenzisa idokhumenti ye-markdown, kufanele iqukathe konke okudingekayo
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  // I-ID eyingqayizivele ye-broadcast
  public int? ID { get; set; }
  
  // I-ID ye-imeyili ehlobene
  public int? EmailId { get; set; }
  
  // Isimo se-broadcast, esethwe ngokuzenzakalelayo ku-"pending"
  public string Status { get; set; } = "pending";
  
  // Igama le-broadcast
  public string? Name { get; set; }
  
  // I-slug ye-broadcast
  public string? Slug { get; set; }
  
  // Ikheli lokuphendula
  public string? ReplyTo { get; set; }
  
  // Ithegi okufanele ithunyelwe kuyo, esethwe ngokuzenzakalelayo ku-"*"
  public string SendToTag { get; set; } = "*";
  
  // Usuku nesikhathi sokudala i-broadcast, esethwe ngokuzenzakalelayo ku-UTC yamanje
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  
  // Umakhi wekilasi oyimfihlo
  private Broadcast()
  {
    
  }
  
  // Indlela ye