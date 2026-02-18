using Microsoft.AspNetCore.Mvc;
using Tailwind.Data;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Tailwind.Mail.Services;

namespace Tailwind.Mail.Api.Admin;

/// <summary>Response returned after attempting a transactional send.</summary>
public class TransactionalEmailResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public CommandResult? Result { get; set; }
}

public class TransactionalRoutes
{
    private TransactionalRoutes() { }

    public static void MapRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/transact", async (
            [FromBody] TransactionalEmailRequest req,
            [FromServices] IDb db,
            [FromServices] IEmailSender sender) =>
        {
            if (string.IsNullOrWhiteSpace(req.To) ||
                string.IsNullOrWhiteSpace(req.Subject) ||
                string.IsNullOrWhiteSpace(req.Html))
            {
                return Results.BadRequest(new TransactionalEmailResponse
                {
                    Success = false,
                    Message = "To, Subject, and Html are required."
                });
            }

            using var conn = db.Connect();
            var cmd = new SendTransactionalEmailCommand(req);
            var result = await cmd.Execute(conn, sender);

            var success = result.Inserted > 0;
            return Results.Ok(new TransactionalEmailResponse
            {
                Success = success,
                Message = success ? $"Email sent to {req.To}" : "Send failed",
                Result = result
            });
        })
        .WithOpenApi(op =>
        {
            op.Summary = "Send a transactional email";
            op.Description = "Sends a single triggered email (e.g. purchase confirmation) directly to a recipient without an unsubscribe link.";
            op.RequestBody.Description = "Recipient address, subject, and HTML body";
            return op;
        })
        .Produces<TransactionalEmailResponse>(StatusCodes.Status200OK)
        .Produces<TransactionalEmailResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
