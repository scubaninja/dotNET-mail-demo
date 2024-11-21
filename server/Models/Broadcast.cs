using Tailwind.Data;
using Dapper;
using System.Data;
namespace Tailwind.Mail.Models;

// प्रसारण बनाने की प्रक्रिया:
// प्रारंभिक डेटा पहले बनाया जाता है (नाम, स्लग)
// फिर ईमेल और अंत में टैग द्वारा भेजने के लिए सेगमेंट
// यदि प्रारंभिक उपयोग मामला एक मार्कडाउन दस्तावेज़ का उपयोग कर रहा है, तो इसमें वह सब कुछ होना चाहिए जो हमें चाहिए
[Table("broadcasts", Schema = "mail")]
public class Broadcast {
  // प्रसारण का ID
  public int? ID { get; set; }
  
  // ईमेल का ID
  public int? EmailId { get; set; }
  
  // प्रसारण की स्थिति, डिफ़ॉल्ट रूप से "pending"
  public string Status { get; set; } = "pending";
  
  // प्रसारण का नाम
  public string? Name { get; set; }
  
  // प्रसारण का slug
  public string? Slug { get; set; }
  
  // reply-to ईमेल पता
  public string? ReplyTo { get; set; }
}