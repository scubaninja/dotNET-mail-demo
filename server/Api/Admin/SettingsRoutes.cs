using Microsoft.AspNetCore.Mvc;
using Tailwind.Data;
using Tailwind.Mail.Commands;

namespace Tailwind.Mail.Api.Admin;

public class DeleteAccountRequest{
  public string? Email { get; set; }
  public bool Confirm { get; set; }
}

public class DeleteAccountResponse{
  public bool Success { get; set; }
  public string? Message { get; set; }
}

public class SettingsRoutes{
  private SettingsRoutes()
  {
  }

  public static void MapRoutes(IEndpointRouteBuilder app)
  {
    app.MapDelete("/admin/settings/account", ([FromBody] DeleteAccountRequest req, [FromServices] IDb db) => {
      if(string.IsNullOrWhiteSpace(req.Email)){
        return Results.BadRequest(new DeleteAccountResponse{
          Success = false,
          Message = "Email is required"
        });
      }

      if(!req.Confirm){
        return Results.BadRequest(new DeleteAccountResponse{
          Success = false,
          Message = "You must confirm the deletion by setting confirm to true"
        });
      }

      using var conn = db.Connect();
      var cmd = new DeleteAccountCommand(req.Email);
      var result = cmd.Execute(conn);

      if(result.Deleted == 0){
        return Results.NotFound(new DeleteAccountResponse{
          Success = false,
          Message = result.Data?.Message ?? "Account not found"
        });
      }

      return Results.Ok(new DeleteAccountResponse{
        Success = true,
        Message = result.Data?.Message ?? "Account permanently deleted"
      });
    }).WithOpenApi(op => {
      op.Summary = "Permanently delete an account";
      op.Description = "Permanently deletes the account associated with the given email address. The user must confirm the action by setting 'confirm' to true in the request body.";
      op.RequestBody.Description = "The email address of the account to delete and a confirmation flag";
      return op;
    })
    .Produces<DeleteAccountResponse>(StatusCodes.Status200OK)
    .Produces<DeleteAccountResponse>(StatusCodes.Status400BadRequest)
    .Produces<DeleteAccountResponse>(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError);
  }
}
