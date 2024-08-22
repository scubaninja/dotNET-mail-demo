using Dapper;
namespace Tailwind.Mail.Models;

[Table("tags", Schema = "mail")]
public class Tag{
  public int? ID { get; set; }
  public string? Slug { get; set; }
  public string? Name { get; set; }
  public string?  Description { get; set; }
  public Tag(string name){
    Name = name;
    Slug = name.ToLower().Replace(" ", "-");
  }
  public Tag(){
    
  }
}

[Table("tagged", Schema = "mail")]
public class Tagged{
  public int? ContactId { get; set; }
  public int? TagId { get; set; }
}