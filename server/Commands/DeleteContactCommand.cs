using System.Data;
using Tailwind.Data;
using Tailwind.Mail.Models;
using Dapper;

namespace Tailwind.Mail.Commands;

public class DeleteContactCommand{
  public int Id { get; set; }
  public DeleteContactCommand(int id)
  {
    Id = id;
  }
  public CommandResult Execute(IDbConnection conn){
    var tx = conn.BeginTransaction();
    try{
      var contact = conn.GetList<Contact>(new {Id=Id}, tx).FirstOrDefault();
      if(contact == null){
        return new CommandResult{
          Data = new{
            Success = false,
            Message = "Contact not found"
          }
        };
      }

      // Delete related records to satisfy FK constraints
      conn.Execute("delete from mail.activity where contact_id = @Id", new {Id}, tx);
      conn.Execute("delete from mail.tagged where contact_id = @Id", new {Id}, tx);
      conn.Execute("delete from mail.subscriptions where contact_id = @Id", new {Id}, tx);
      conn.Delete(contact, tx);

      tx.Commit();

      return new CommandResult{
        Deleted = 1,
        Data = new{
          Success = true,
          Message = "Account permanently deleted"
        }
      };
    }catch(Exception){
      tx.Rollback();
      return new CommandResult{
        Deleted = -1,
        Data = new{
          Success = false,
          Message = "An error occurred while deleting the account"
        }
      };
    }
  }
}
