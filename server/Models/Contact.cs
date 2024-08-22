
using Dapper;
namespace Tailwind.Mail.Models;

public class SignUpRequest
{
  public string Name { get; set; }
  public string Email { get; set; }
}
[Table("contacts", Schema = "mail")]
public class Contact
{
  public string Name { get; set; }
  public string Email { get; set; }
  public bool Subscribed { get; set; }
  public string Key { get; set; } = Guid.NewGuid().ToString();
  public int? ID { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  
  public Contact()
  {
    
  }
  public Contact(string name, string email)
  {
    Name = name;
    Email = email;
  }

}