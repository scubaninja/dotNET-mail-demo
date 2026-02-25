using System.Data;
using Tailwind.Data;
using Tailwind.Mail.Models;
using Dapper;

namespace Tailwind.Mail.Commands;

public class DeleteAccountCommand{
  public string Email { get; set; }
  public DeleteAccountCommand(string email)
  {
    Email = email;
  }
  public CommandResult Execute(IDbConnection conn){
    var tx = conn.BeginTransaction();
    try{
      var contact = conn.GetList<Contact>(new {Email = Email}, tx).FirstOrDefault();
      if(contact == null){
        return new CommandResult{
          Data = new{
            Success = false,
            Message = "Account not found"
          }
        };
      }

      // Delete related records first (activity, tagged)
      conn.Execute("delete from mail.activity where contact_id = @Id", new {contact.ID}, tx);
      conn.Execute("delete from mail.tagged where contact_id = @Id", new {contact.ID}, tx);
      conn.Execute("delete from mail.subscriptions where contact_id = @Id", new {contact.ID}, tx);
      conn.Delete(contact, tx);

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
