//public endpoints for subscribe/unsubscribe
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tailwind.Data;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Api;

public class PublicRoutes{

  public static void MapRoutes(IEndpointRouteBuilder app)
  {

    //public routes
    app.MapGet("/about", () => "Tailwind Traders Mail Services API").WithOpenApi(op => {
      op.Summary = "📋 Get API Information";
      op.Description = "Returns basic information about the Tailwind Traders Mail Services API, including version and capabilities.";
      return op;
    }).WithTags("Public");

    app.MapGet("/unsubscribe/{key}", (string key, [FromServices] IDb db) => {
      using(var conn = db.Connect()){
        var cmd = new ContactOptOutCommand(key);
        var result = cmd.Execute(conn);
        return result.Updated > 0;
      }
    }).WithOpenApi(op => {
      op.Summary = "🚫 Unsubscribe Contact";
      op.Description = "Allows a contact to opt-out from the mailing list using their unique unsubscribe key. This key is typically included in email footers.";
      op.Parameters[0].Description = "The contact's unique unsubscribe key (provided in email communications)";
      return op;
    }).WithTags("Public");

    //this isn't implemented yet in terms of data
    app.MapGet("/link/clicked/{key}", (string key, [FromServices] IDb db) => {
      var cmd = new LinkClickedCommand(key);
      var result = cmd.Execute();
      return result;
    }).WithOpenApi(op => {
      op.Summary = "🔗 Track Link Click";
      op.Description = "Records when a contact clicks a tracked link in an email. This endpoint collects engagement metrics for broadcasts and email sequences.";
      op.Parameters[0].Description = "The unique tracking key for the clicked link (embedded in email links)";
      return op;
    }).WithTags("Public");

    app.MapPost("/signup", async ([FromBody] SignUpRequest req,  [FromServices] IDb db) => {
      var contact = new Contact{
        Email = req.Email,
        Name = req.Name
      };
      using var conn = db.Connect();
      var result = await conn.ExecuteAsync("insert into contacts (email, name) values (@Email, @Name)", contact);
      return result;
      
    }).WithOpenApi(op => {
      op.Summary = "✉️ Sign Up New Contact";
      op.Description = "Adds a new contact to the mailing list. This endpoint is typically used by website signup forms to collect new subscribers.";
      op.RequestBody.Description = "Contact information including name and email address";
      return op;
    }).WithTags("Public");
  }

}