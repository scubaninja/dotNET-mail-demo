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
      op.Summary = "Find one or more contacts using a fuzzy match on email or name";
      op.Description = "Find a set of contacts using a search term";
      return op;
    }).Produces<ContactSearchResponse>()
    .Produces(500);
  }
}
public interface IQuantifiedList{
  public IDictionary<string,int> Items {get; set;}
  public bool AddItem(string sku, int quantity);
  public bool RemoveItem(string sku, int quantity);
}