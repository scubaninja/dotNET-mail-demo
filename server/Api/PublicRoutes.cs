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
      op.Summary = "Information about the API";
      op.Description = "This is the API for the Tailwind Traders Mail Services API";
      return op;
    });

    app.MapGet("/unsubscribe/{key}", (string key, [FromServices] IDb db) => {
      using(var conn = db.Connect()){
        var cmd = new ContactOptOutCommand(key);
        var result = cmd.Execute(conn);
        return result.Updated > 0;
      }
    }).WithOpenApi(op => {
      op.Summary = "Unsubscribe from the mailing list";
      op.Description = "This is the API for the Tailwind Traders Mail Services API";
      op.Parameters[0].Description = "This is the contact's unique key";
      return op;
    });

    //this isn't implemented yet in terms of data
    app.MapGet("/link/clicked/{key}", (string key, [FromServices] IDb db) => {
      var cmd = new LinkClickedCommand(key);
      var result = cmd.Execute();
      return result;
    }).WithOpenApi(op => {
      op.Summary = "Track a link click";
      op.Description = "This adds to the stats for a given email in a broadcast or a sequence";
      op.Parameters[0].Description = "This is the link's unique key";
      return op;
    });

    app.MapPost("/signup", async ([FromBody] SignUpRequest req,  [FromServices] IDb db) => {
      var contact = new Contact{
        Email = req.Email,
        Name = req.Name
      };
      using var conn = db.Connect();
      var result = await conn.ExecuteAsync("insert into contacts (email, name) values (@Email, @Name)", contact);
      return result;
      
    }).WithOpenApi(op => {
      op.Summary = "Sign up for the mailing list";
      op.Description = "This is the form endpoint for signing up for the mail list";
      op.RequestBody.Description = "This is the contact's information";
      return op;
    });
  }

}