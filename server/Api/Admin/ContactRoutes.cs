using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tailwind.Data;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Api.Admin;

public class ContactSearchResponse{
  public string? Term { get; set; }
  public IEnumerable<Contact> Contacts { get; set; } = new List<Contact>();
}

public class ContactRoutes{
  private ContactRoutes()
  {
    
  }
  public static void MapRoutes(IEndpointRouteBuilder app)
  {
    //CRUD for contacts
    //Tagging
    //Search
    app.MapGet("/admin/contacts/search", ([FromQuery] string term, [FromServices] IDb db) => {
      //searches by both email and name
      var response = new ContactSearchResponse{Term = term};
      var sql = "select * from mail.contacts where email ~* @term or name ~* @term";
      using var conn = db.Connect();
      response.Contacts = conn.Query<Contact>(sql, new {term});
      return response;
    }).WithOpenApi(op => {
      op.Summary = "🔍 Search Contacts";
      op.Description = "Find contacts using fuzzy search on email or name fields. Returns all matching contacts for the provided search term.";
      op.Parameters[0].Description = "Search term to match against contact names or email addresses";
      return op;
    }).Produces<ContactSearchResponse>()
    .Produces(500)
    .WithTags("Admin - Contacts");
  }
}
public interface IQuantifiedList{
  public IDictionary<string,int> Items {get; set;}
  public bool AddItem(string sku, int quantity);
  public bool RemoveItem(string sku, int quantity);
}