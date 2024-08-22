using Xunit;
using Tailwind.Data;
using Tailwind.Mail.Commands;
using Markdig;
using Markdig.Syntax;
using Tailwind.Mail.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tailwind.Mail.Api.Admin;

namespace Tailwind.Mail.Tests;

//probably end up mocking this at some point
//for now, just use the test db


[Collection("Bulk tags")]
public class BulkTagTests:TestBase
{
  public BulkTagTests()
  {
  }
  [Fact]
  public void Bulk_tagging_existing_emails_should_update_100_contacts()
  {
    for(var i = 0; i < 100; i++){
      var c = new Contact{
        Email = $"test-tags-{i}@test.com",
        Name = $"Test Tags {i}",
        Subscribed = true
      };
      Conn.Insert(c);
    };
    var emails = Conn.Query<string>("select email from mail.contacts limit 100");
    var req = new BulkTagRequest{
      Tag = "Bulk Test",
      Emails = emails
    };
    var result = new BulkTagCommand{
      Tag = req.Tag,
      Emails = req.Emails
    }.Execute(Conn);

    Assert.True(result.Updated >= 100);
    
    //we should be not get an error here
    result = new BulkTagCommand{
      Tag = req.Tag,
      Emails = req.Emails
    }.Execute(Conn);

    Assert.Equal(100, result.Updated);
  }
  [Fact]
  public void Bulk_tagging_new_emails_should_insert_100_contacts()
  {
    var emails = new List<string>();
    for(var i = 0; i < 100; i++){
      emails.Add($"bulk{i}@test.com");
    }
    var req = new BulkTagRequest{
      Tag = "Bulk Test",
      Emails = emails
    };
    var result = new BulkTagCommand{
      Tag = req.Tag,
      Emails = req.Emails
    }.Execute(Conn);

    Assert.Equal(100, result.Inserted);
  }
}