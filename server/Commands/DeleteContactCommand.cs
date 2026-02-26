using System.Data;
using Tailwind.Data;
using Tailwind.Mail.Models;
using Dapper;

namespace Tailwind.Mail.Commands;

public class DeleteContactCommand{
  public string Key { get; set; }
  public DeleteContactCommand(string key)
  {
    Key = key;
  }
  public CommandResult Execute(IDbConnection conn){
    var tx = conn.BeginTransaction();
    try{
      var contact = conn.GetList<Contact>(new {Key=Key},tx).FirstOrDefault();
      if(contact == null){
        return new CommandResult{
          Data = new{
            Success = false,
            Message = "Contact not found"
          }
        };
      }

      var id = contact.ID;
      conn.Execute("delete from mail.subscriptions where contact_id = @id", new {id}, tx);
      conn.Execute("delete from mail.tagged where contact_id = @id", new {id}, tx);
      conn.Execute("delete from mail.activity where contact_id = @id", new {id}, tx);
      conn.Execute("delete from mail.contacts where id = @id", new {id}, tx);

      tx.Commit();

      return new CommandResult{
        Deleted = 1,
        Data = new{
          Success = true,
          Message = "Account permanently deleted"
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
