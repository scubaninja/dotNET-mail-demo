using System.Data;
using Dapper;
using Tailwind.Data;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Commands
{
  public class BulkTagCommand
  {
    public string? Tag { get; set; }
    public IEnumerable<string> Emails { get; set; } = new List<string>();
    public CommandResult Execute(IDbConnection conn)
    {
      var tx = conn.BeginTransaction();
      var updated = 0;
      var inserted = 0;
      try
      {
        var tag = conn.GetList<Tag>(new { name = Tag }, tx).FirstOrDefault();
        if (tag == null)
        {
          tag = new Tag(Tag);
          tag.ID = conn.Insert(tag, tx);
        }
        foreach (var email in Emails)
        {
          var contact = conn.GetList<Contact>(new { Email = email }).FirstOrDefault();
          if (contact == null)
          {
            //create the contact
            contact = new Contact
            {
              Email = email,
              Subscribed = true
            };
            inserted++;
            contact.ID = conn.Insert(contact, tx);
          }
          else
          {
            updated++;
          }
          var sql = @"
          insert into mail.tagged (contact_id, tag_id) 
          values (@contactId, @tagId) 
          on conflict do nothing"; //upsert if already tagged
          conn.Execute(sql, new { contactId = contact.ID, tagId = tag.ID }, tx);
        }
        tx.Commit();
        return new CommandResult { Inserted = inserted, Updated = updated };
      }
      catch (Exception e)
      {
        tx.Rollback();
        return new CommandResult { Data = new { error = e.Message } };
      }
    }
  }

}