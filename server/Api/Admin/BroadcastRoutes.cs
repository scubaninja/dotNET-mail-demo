//API bits for the admin CLI//public endpoints for subscribe/unsubscribe
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Tailwind.AI;
using Tailwind.Data;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Api.Admin;

public class ValidationResponse{
  public bool Valid { get; set; }
  public string Message { get; set; }
  public long Contacts { get; set; }
  public MarkdownEmail? Data { get; set; }
  public ValidationResponse()
  {
    Message = "The markdown is valid";
  }
}
public class ValidationRequest{
  public string? Markdown { get; set; }
}
public class ChatRequest{
  public string? Prompt { get; set; }
}
public class ChatResponse{
  public bool Success { get; set; }
  public string? Prompt { get; set; }
  public string? Reply { get; set; }

}
public class QueueBroadcastResponse{
  public bool Success { get; set; }
  public string? Message { get; set; }
  public CommandResult? Result { get; set; }
}

public class BroadcastRoutes{
  private BroadcastRoutes()
  {
    
  }
  //all of these routes will be protected in some way...
  public static void MapRoutes(IEndpointRouteBuilder app)
  {
    //queue up a broadcast

    //CRUD for email templates
    //Message queue problems - failed, bounced
    //Message queue pending
    //Broadcast summary
    //Contact stats
    //Tag stats

    app.MapPost("/admin/get-chat", async ([FromBody] ChatRequest req,  [FromServices] IDb db) => {
      
      //this should already be validated but...
      if(req.Prompt == null){
        return new ChatResponse{
          Success = false,
          Prompt = req.Prompt,
          Reply = "Ensure there is a Subject and Prompt in the request",
        };
      }else{
        var chat = new Chat();
        var res = await chat.Prompt(req.Prompt);
        return new ChatResponse{
          Success = true,
          Prompt = req.Prompt,
          Reply = res
        };
      }

      //return response;
    }).WithOpenApi(op => {
      op.Summary = "🤖 AI Chat Assistant";
      op.Description = "Get AI-powered assistance for creating email content. Send a prompt and receive intelligent suggestions for your email campaigns.";
      op.RequestBody.Description = "Your prompt or question for the AI assistant";
      return op;
    }).WithTags("Admin - AI Assistant");
    //validate a broadcast
    app.MapPost("/admin/queue-broadcast", ([FromBody] ValidationRequest req,  [FromServices] IDb db) => {
      var mardown = req.Markdown;
      var doc = MarkdownEmail.FromString(req.Markdown);
      //this should already be validated but...
      if(!doc.IsValid()){
        return new QueueBroadcastResponse{
          Success = false,
          Message = "Ensure there is a Body, Subject and Summary in the markdown",
        };
      }
      using var conn = db.Connect();
      var res = new CreateBroadcast(doc).Execute(conn);
      //ensure that it has a subject, summary, and slug
      var response = new QueueBroadcastResponse{
        Success = res.Inserted > 0,
        Message = $"The broadcast was queued with ID {res.Data.BroadcastId} and {res.Inserted} messages were created",
        Result = res
      };

      return response;
    }).WithOpenApi(op => {
      op.Summary = "📤 Queue Email Broadcast";
      op.Description = "Creates and queues a new broadcast email campaign from markdown content. Messages will be sent to all opted-in contacts. The markdown must include Subject, Summary, and Body sections.";
      op.RequestBody.Description = "The markdown content for the email broadcast";
      return op;
    }).WithTags("Admin - Broadcasts");

    app.MapPost("/admin/validate", ([FromBody] ValidationRequest req,  [FromServices] IDb db) => {
      if(req.Markdown == null){
        return new ValidationResponse{
          Valid = false,
          Message = "The markdown is null"
        };
      }
      var doc = MarkdownEmail.FromString(req.Markdown);
      if(!doc.IsValid()){
        return new ValidationResponse{
          Valid = false,
          Message = "Ensure there is a Subject and Summary in the markdown",
          Data = doc
        };
      }
      var broadcast = Broadcast.FromMarkdownEmail(doc);
      //how many contacts?
      using var conn = db.Connect();
      var contacts = broadcast.ContactCount(conn);
      //ensure that it has a subject, summary, and slug
      var response = new ValidationResponse{
        Valid = true,
        Data = doc,
        Contacts = contacts
      };
      return response;
    })
    .WithOpenApi(op => {
      op.Summary = "✅ Validate Email Markdown";
      op.Description = "Validates markdown content for a broadcast email before sending. Returns validation status and estimated contact count. Use this to catch errors before queuing your broadcast.";
      op.RequestBody.Description = "The markdown content to validate";
      return op;
    })
    .Produces<ValidationResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status403Forbidden)
    .Produces(StatusCodes.Status500InternalServerError)
    .WithTags("Admin - Broadcasts");
  }

}