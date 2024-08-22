using Tailwind.Mail.Models;
using Tailwind.Data;
using Dapper;
using System.Data;

namespace Tailwind.Mail.Commands;

public class ContactOptinCommand{
  public Contact Contact { get; set; }
  public ContactOptinCommand(Contact contact)
  {
    Contact = contact;
  }
  public CommandResult Execute(IDbConnection conn){
    
    var tx = conn.BeginTransaction();
    try{
      Contact.Subscribed = true;
      conn.Update(Contact,tx);
      conn.Insert(new Activity{
        ContactId = Contact.ID,
        Key = "optin",
        Description="Opted in using key"
      },tx);
      tx.Commit();

      return new CommandResult{
        Updated = 1,
        Data = new{
          Success = true,
          Message = "Contact subscribed"
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