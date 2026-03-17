using System.Data;
using System.Net.Mail;
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

public class CreateUserRequest{
  public string? Email { get; set; }
  public string? Name { get; set; }
  public string? Role { get; set; }
}

public class CreateUserResponse{
  public bool Success { get; set; }
  public string? Message { get; set; }
  public int? Id { get; set; }
  public string? Key { get; set; }
  public string? Email { get; set; }
  public string? Role { get; set; }
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

    app.MapPost("/admin/contacts", ([FromBody] CreateUserRequest req, [FromServices] IDb db) => {
      if(string.IsNullOrWhiteSpace(req.Email)){
        return Results.BadRequest(new CreateUserResponse{
          Success = false,
          Message = "Email is required"
        });
      }
      try{
        var _ = new MailAddress(req.Email);
      }catch(FormatException){
        return Results.BadRequest(new CreateUserResponse{
          Success = false,
          Message = "A valid email address is required"
        });
      }
      if(string.IsNullOrWhiteSpace(req.Role)){
        return Results.BadRequest(new CreateUserResponse{
          Success = false,
          Message = "Role is required"
        });
      }

      var contact = new Contact{
        Email = req.Email,
        Name = req.Name ?? string.Empty,
        Subscribed = true
      };

      using var conn = db.Connect();
      var existing = conn.GetList<Contact>(new {Email = req.Email});
      if(existing.Any()){
        return Results.BadRequest(new CreateUserResponse{
          Success = false,
          Message = "A user with this email already exists"
        });
      }

      using var tx = conn.BeginTransaction();
      try{
        var id = conn.Insert(contact, tx);
        conn.Insert(new Activity{
          ContactId = id,
          Key = "admin-created",
          Description = $"Contact created by admin with role: {req.Role}"
        }, tx);
        tx.Commit();
        return Results.Created($"/admin/contacts/{id}", new CreateUserResponse{
          Success = true,
          Message = "User created successfully",
          Id = id,
          Key = contact.Key,
          Email = contact.Email,
          Role = req.Role
        });
      }catch(Exception){
        tx.Rollback();
        return Results.BadRequest(new CreateUserResponse{
          Success = false,
          Message = "An error occurred while creating the user. Please try again."
        });
      }
    }).WithOpenApi(op => {
      op.Summary = "Create a new contact/user";
      op.Description = "Creates a new contact in the system with the provided details. Email and role are required.";
      op.RequestBody.Description = "The user details. Email and role are required fields.";
      return op;
    }).Produces<CreateUserResponse>(StatusCodes.Status201Created)
    .Produces<CreateUserResponse>(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError);

    app.MapGet("/admin/contacts/{id:int}", (int id, [FromServices] IDb db) => {
      using var conn = db.Connect();
      var contact = conn.Get<Contact>(id);
      if(contact == null){
        return Results.NotFound(new { Message = $"Contact with ID {id} not found" });
      }
      return Results.Ok(contact);
    }).WithOpenApi(op => {
      op.Summary = "Get a contact by ID";
      op.Description = "Retrieves a single contact by their unique ID";
      op.Parameters[0].Description = "The contact's unique ID";
      return op;
    }).Produces<Contact>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError);
  }
}
public interface IQuantifiedList{
  public IDictionary<string,int> Items {get; set;}
  public bool AddItem(string sku, int quantity);
  public bool RemoveItem(string sku, int quantity);
}