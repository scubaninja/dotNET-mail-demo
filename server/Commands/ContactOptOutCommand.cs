using System.Data;
using Tailwind.Data;
using Tailwind.Mail.Models;
using Dapper;

namespace Tailwind.Mail.Commands;

public class ContactOptOutCommand{
  public string Key { get; set; }
  public ContactOptOutCommand(string key)
  {
    Key = key;
  }
  public CommandResult Execute(IDbConnection conn){
    
    var tx = conn.BeginTransaction();
    //get the contact
    try{
      var contact = conn.GetList<Contact>(new {Key=Key},tx).FirstOrDefault();
      if(contact != null){
        contact.Subscribed = false;
        conn.Update(contact,tx);
        conn.Insert(new Activity{
          ContactId = contact.ID,
          Key = "optout",
          Description="Unsubbed"
        },tx);
      }else{
        return new CommandResult{
          Data = new{
            Success = false,
            Message = "Contact not found"
          }
        };
      }

      tx.Commit();

      return new CommandResult{
        Updated = 1,
        Data = new{
          Success = true,
          Message = "Contact unsubscribed"
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