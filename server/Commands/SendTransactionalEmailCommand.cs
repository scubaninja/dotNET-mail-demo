using System.Data;
using Dapper;
using Tailwind.Data;
using Tailwind.Mail.Api.Admin;
using Tailwind.Mail.Models;
using Tailwind.Mail.Services;

namespace Tailwind.Mail.Commands;

/// <summary>
/// Sends a single transactional email directly to a recipient without an unsubscribe link.
/// Use this for purchase confirmations, password resets, and other triggered emails.
/// </summary>
public class SendTransactionalEmailCommand
{
    public TransactionalEmailRequest Request { get; set; }

    public SendTransactionalEmailCommand(TransactionalEmailRequest request)
    {
        Request = request;
    }

    public async Task<CommandResult> Execute(IDbConnection conn, IEmailSender sender)
    {
        var config = Viper.Config();
        var defaultFrom = config.Get("DEFAULT_FROM") ?? "noreply@tailwind.dev";

        var message = new Message
        {
            Source = "transactional",
            Slug = $"transact-{Guid.NewGuid()}",
            SendTo = Request.To,
            SendFrom = Request.From ?? defaultFrom,
            Subject = Request.Subject,
            Html = Request.Html,
            SendAt = DateTimeOffset.UtcNow
        };

        try
        {
            var sent = await sender.Send(message);
            var id = await conn.InsertAsync(sent);

            return new CommandResult
            {
                Inserted = 1,
                Data = new
                {
                    Success = true,
                    MessageId = id,
                    SentTo = sent.SendTo,
                    SentAt = sent.SentAt
                }
            };
        }
        catch (Exception e)
        {
            return new CommandResult
            {
                Data = new
                {
                    Success = false,
                    Message = e.Message
                }
            };
        }
    }
}
