using System.Data;
using Tailwind.Data;
using Tailwind.Mail.Models;
using Dapper;

namespace Tailwind.Mail.Commands;

public class ContactSignupCommand{
  public Contact Contact { get; set; }
  public ContactSignupCommand(Contact contact)
  {
    Contact = contact;
  }
  public CommandResult Execute(IDbConnection conn){
    var tx = conn.BeginTransaction();
    
    var contacts = conn.GetList<Contact>(new {Email=Contact.Email}, tx);
    if(contacts.Count() > 0){
      return new CommandResult{
        Data = new{
          Success=false,
          Message = "User exists"
        }
      };
    }
    try{
      var id = conn.Insert(Contact, tx);
      conn.Insert(new Activity{
        ContactId = id,
        Key = "signup",
        Description="New Contact"
      },tx);
      
      tx.Commit();
      return new CommandResult{
        Inserted = 1,
        Data = new{
          Success=true,
          ID = id,
          Key=Contact.Key,
          Subscribed = Contact.Subscribed
        }
      };
    }catch(Exception e){
      tx.Rollback();
      return new CommandResult{
        Data = new{
          Success = false,
          Message = e.Message
        }
      };
    }   
  }
}