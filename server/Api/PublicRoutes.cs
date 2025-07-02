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

    app.MapPost("/signup", async ([FromBody] SignUpRequest req, HttpContext context, [FromServices] IDb db) => {
      if (string.IsNullOrEmpty(req.Email) || !IsValidEmail(req.Email))
      {
        return Results.BadRequest(new { success = false, message = "Valid email address is required" });
      }
      
      if (!req.ConsentToProcessing)
      {
        return Results.BadRequest(new { success = false, message = "Consent to data processing is required" });
      }
      
      var contact = new Contact{
        Email = req.Email,
        Name = req.Name,
        ConsentTimestamp = DateTimeOffset.UtcNow,
        ConsentSource = req.ConsentSource,
        ConsentIPAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
        PrivacyPolicyVersion = "1.0", // Get from configuration in production
        DataRetentionExpiry = DateTimeOffset.UtcNow.AddYears(2) // 2-year retention by default
      };
      
      try {
        using var conn = db.Connect();
        // Use a transaction for consistency
        using var transaction = conn.BeginTransaction();
        
        try {
          // Check if email already exists
          var existingContact = conn.QueryFirstOrDefault<Contact>("SELECT * FROM mail.contacts WHERE email = @Email", new { contact.Email }, transaction);
          
          if (existingContact != null)
          {
            transaction.Rollback();
            return Results.BadRequest(new { success = false, message = "Email already registered" });
          }
          
          // Insert the contact
          var result = await conn.ExecuteAsync(
            @"INSERT INTO mail.contacts 
              (email, name, consent_timestamp, privacy_policy_version, consent_source, consent_ip_address, data_retention_expiry) 
              VALUES 
              (@Email, @Name, @ConsentTimestamp, @PrivacyPolicyVersion, @ConsentSource, @ConsentIPAddress, @DataRetentionExpiry)
              RETURNING id", contact, transaction);
          
          // Log the consent in activity table
          await conn.ExecuteAsync(
            @"INSERT INTO mail.activity
              (contact_id, key, description)
              VALUES
              (@ContactId, 'consent', 'Explicit consent provided for data processing')",
            new { ContactId = contact.ID, }, transaction);
            
          // Log the data processing activity
          await conn.ExecuteAsync(
            @"INSERT INTO mail.data_processing_activities
              (contact_id, activity_type, description, performed_by, ip_address)
              VALUES
              (@ContactId, 'create', 'Contact created with explicit consent', 'web-signup', @IpAddress)",
            new { ContactId = contact.ID, IpAddress = contact.ConsentIPAddress }, transaction);
            
          transaction.Commit();
          
          return Results.Ok(new { success = true, message = "Successfully signed up, please check your email for confirmation" });
        }
        catch (Exception) {
          transaction.Rollback();
          throw;
        }
      }
      catch (Exception) {
        return Results.Problem(
          title: "Error processing signup",
          detail: "There was an error processing your request. Please try again later.",
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }).WithOpenApi(op => {
      op.Summary = "Sign up for the mailing list";
      op.Description = "This is the form endpoint for signing up for the mail list with explicit consent (GDPR compliant)";
      op.RequestBody.Description = "This is the contact's information including explicit consent";
      return op;
    });
    
    // Helper method for email validation
    static bool IsValidEmail(string email)
    {
      try {
        var addr = new System.Net.Mail.MailAddress(email);
        return addr.Address == email;
      }
      catch {
        return false;
      }
    }
  }

}