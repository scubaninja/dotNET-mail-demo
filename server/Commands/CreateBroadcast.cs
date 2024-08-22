using System.Data;
using Tailwind.Data;
using Tailwind.Mail.Models;
using Dapper;
namespace Tailwind.Mail.Commands;


public class CreateBroadcast
{
  private Broadcast _broadcast { get; set; }
  public Email _email { get; set; }
  public CreateBroadcast(MarkdownEmail email)
  {
    _email = new Email(email);
    _broadcast = Broadcast.FromMarkdownEmail(email);
  } 
  
  //TODO: Clean this up
  public CommandResult Execute(IDbConnection conn){

    var from = Viper.Config().Get("DEFAULT_FROM");
    if(String.IsNullOrEmpty(from)){
      from="noreply@tailwind.dev";
    }


    var tx = conn.BeginTransaction();
    try{
      //save the email
      var emailId = conn.Insert(_email, tx);
      _broadcast.EmailId = emailId;
      _broadcast.ReplyTo = from;
      //create the broadcast
      var broadcastId = conn.Insert(_broadcast, tx);

      //create the messages - glorious sql, isn't it?
      //makes me want to write a haiku using copilot, all about SQL:
      //select from where and join
      //insert update delete
      //sql is the best
      var sql = @"
          insert into mail.messages (source, slug, send_to, send_from, subject, html, send_at)
          select 'broadcast', @slug, mail.contacts.email, @reply_to, @subject, @html, now() 
          from mail.contacts
        ";
        
      int messagesCreated;


      if(_broadcast.SendToTag != "*"){
        sql += @"
        inner join mail.tagged on mail.tagged.contact_id = mail.contacts.id
        inner join mail.tags on mail.tags.id = mail.tagged.tag_id
        where subscribed = true
        and mail.tags.slug = @tagSlug
        ";

        messagesCreated = conn.Execute(sql, new{
          broadcastId,
          tagSlug = _broadcast.SendToTag,
          slug = _email.Slug,
          reply_to = from,
          subject = _email.Subject,
          html = _email.Html
        });
      }else{
        sql+="where subscribed = true";
        messagesCreated = conn.Execute(sql, new{
          broadcastId,
          slug = _email.Slug,
          reply_to = from,
          subject = _email.Subject,
          html = _email.Html
        });
      }
      
      conn.Execute("NOTIFY broadcasts, '@slug'", new {slug=_broadcast.Slug}, tx);
      tx.Commit();

      return new CommandResult{
        Data = new{
          BroadcastId = broadcastId,
          EmailId = emailId,
          Notified = true
        },
        Inserted = messagesCreated
      };
    }catch(Exception e){
      tx.Rollback();
      throw e;
    }

  }
}