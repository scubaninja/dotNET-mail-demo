using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tailwind.Data;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Api.Admin;

public class BulkTagRequest{
  public bool Success { get; set; } = false;
  public string Message { get; set; }
  public string Tag { get; set; }
  public IEnumerable<string> Emails { get; set; }
}
public class BulkTagResponse{
  public bool Success { get; set; } = false;
  public string Message { get; set; } = "No response";
  public int Created { get; set; }
  public int Updated { get; set; }
}

public class BulkOperationRoutes{
  private BulkOperationRoutes()
  {
    
  }
  public static void MapRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost("/admin/bulk/contacts/tag", ([FromBody] BulkTagRequest request, [FromServices] IDb db) => {
      using var conn = db.Connect();
      if(String.IsNullOrEmpty(request.Tag) || request.Emails.Count() == 0){
        return new BulkTagResponse{Success = false, Message = "Be sure to include a tag and at least one email address and a tag"};
      }
      //there's a chance of sending in multiple tags... so... we'll just do one at a time
      var tags = request.Tag.Split(",");
      var totalInserted = 0;
      var totalUpdated = 0;
      foreach(var tag in tags){
        var result = new BulkTagCommand{
          Tag = tag.Trim(),
          Emails = request.Emails
        }.Execute(conn);
      }
      return new BulkTagResponse{
        Success = true, 
        Created = totalInserted, 
        Updated = totalUpdated,
        Message = $"{tags.Count()} Tag(s) applied to {request.Emails.Count()} contacts"  
      };
    }).WithOpenApi(op => {
      op.Summary = "🏷️ Bulk Tag Contacts";
      op.Description = "Apply one or more tags to multiple contacts at once. Useful for organizing contacts into segments for targeted campaigns. Separate multiple tags with commas.";
      op.RequestBody.Description = "List of email addresses and tag(s) to apply";
      return op;
    }).Produces<CommandResult>()
    .Produces(500)
    .WithTags("Admin - Bulk Operations");
  }
}