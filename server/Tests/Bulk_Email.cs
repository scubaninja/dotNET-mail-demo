using Xunit;
using Tailwind.Data;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tailwind.Mail.Api.Admin;
using Tailwind.Mail.Services;

namespace Tailwind.Mail.Tests;

//probably end up mocking this at some point
//for now, just use the test db


[Collection("Bulk email")]
public class BulkEmailTest:TestBase
{
  MailHogSender _outbox;
  IList<Message> _queue;
  public BulkEmailTest()
  {
    _outbox = new MailHogSender();
  }
  // [Fact]
  // public async void Bulk_email_sending(){
  //   List<Message> mssgs = new List<Message>();
  //   var limit = 10000;
  //   Console.WriteLine("Creating 10K messages");
  //   Console.WriteLine(DateTime.Now);
  //   for(var i = 0; i <= limit; i++)
  //   {
  //     var m = new Message{
  //       SendTo = $"test{i}-bulk@test.com",
  //       SendFrom = "thing@dev.null",
  //       Subject = "Bulk Send Test",
  //       Html = "<h1>Test</h1>",
  //       Slug = "bulk-send-test",
  //       SendAt = DateTimeOffset.UtcNow
  //     };
  //     m.ID = await Conn.InsertAsync(m);
  //     mssgs.Add(m);
  //   }
  //   Console.WriteLine("Adding to send queue");
  //   var sent = await _outbox.SendBulk(mssgs);
  //   Console.WriteLine("Sent");
  //   Console.WriteLine(DateTime.Now);
  //   Assert.True(sent >= limit -1); //off by one error? weird.
  // }
}