using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tailwind.Data;
using Tailwind.Mail.Models;
using System.Text.Json;

namespace Tailwind.Mail.Api;

/// <summary>
/// Endpoints for handling GDPR data subject requests including data access, export, and deletion.
/// </summary>
public class DataSubjectRequestsRoutes
{
    private DataSubjectRequestsRoutes()
    {
    }

    public static void MapRoutes(IEndpointRouteBuilder app)
    {
        // Get all data about a user (Data Subject Access Request)
        app.MapGet("/data-subject/export/{key}", async (string key, [FromServices] IDb db) =>
        {
            try
            {
                using var conn = db.Connect();
                
                // First verify the key is valid
                var contact = await conn.QueryFirstOrDefaultAsync<Contact>(
                    "SELECT * FROM mail.contacts WHERE key = @key", 
                    new { key });
                
                if (contact == null)
                {
                    return Results.NotFound(new { success = false, message = "Contact not found" });
                }
                
                // Create a comprehensive data export
                var export = new Dictionary<string, object>();
                
                // Basic contact info
                export["contact"] = contact;
                
                // Get tags
                var tags = await conn.QueryAsync<string>(
                    @"SELECT t.name 
                      FROM mail.tags t
                      JOIN mail.tagged tg ON t.id = tg.tag_id
                      WHERE tg.contact_id = @id", 
                    new { id = contact.ID });
                export["tags"] = tags;
                
                // Get activity history
                var activities = await conn.QueryAsync<Activity>(
                    "SELECT * FROM mail.activity WHERE contact_id = @id ORDER BY created_at DESC",
                    new { id = contact.ID });
                export["activities"] = activities;
                
                // Get subscription data
                var subscriptions = await conn.QueryAsync<dynamic>(
                    @"SELECT s.name, s.description, sub.created_at as subscribed_at
                      FROM mail.subscriptions sub
                      JOIN mail.sequences s ON sub.sequence_id = s.id
                      WHERE sub.contact_id = @id",
                    new { id = contact.ID });
                export["subscriptions"] = subscriptions;
                
                // Get message history - emails sent to this contact
                var messages = await conn.QueryAsync<dynamic>(
                    @"SELECT subject, send_from, sent_at, source, slug
                      FROM mail.messages
                      WHERE send_to = @email
                      ORDER BY created_at DESC",
                    new { email = contact.Email });
                export["messages_received"] = messages;

                // Log this access request
                await conn.ExecuteAsync(
                    @"INSERT INTO mail.data_processing_activities
                      (contact_id, activity_type, description, performed_by)
                      VALUES
                      (@contactId, 'access', 'Data subject access request fulfilled', 'system')",
                    new { contactId = contact.ID });
                
                return Results.Ok(new 
                { 
                    success = true, 
                    contact = contact.Email,
                    exportedAt = DateTimeOffset.UtcNow,
                    data = export 
                });
            }
            catch (Exception)
            {
                return Results.Problem(
                    title: "Error processing data export request",
                    detail: "There was an error processing your data export request. Please try again later.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).WithOpenApi(op =>
        {
            op.Summary = "Export all user data (GDPR Data Subject Access Request)";
            op.Description = "Returns all data stored about a user identified by their unique key";
            op.Parameters[0].Description = "The user's unique identification key";
            return op;
        });

        // Request data deletion (Right to be Forgotten)
        app.MapDelete("/data-subject/delete/{key}", async (string key, [FromServices] IDb db) =>
        {
            try
            {
                using var conn = db.Connect();
                
                // First verify the key is valid
                var contact = await conn.QueryFirstOrDefaultAsync<Contact>(
                    "SELECT * FROM mail.contacts WHERE key = @key", 
                    new { key });
                
                if (contact == null)
                {
                    return Results.NotFound(new { success = false, message = "Contact not found" });
                }
                
                // Execute the deletion function
                await conn.ExecuteAsync(
                    "SELECT mail.delete_contact_data(@id)",
                    new { id = contact.ID });
                
                return Results.Ok(new 
                { 
                    success = true, 
                    message = "Your data has been deleted in accordance with GDPR requirements" 
                });
            }
            catch (Exception)
            {
                return Results.Problem(
                    title: "Error processing deletion request",
                    detail: "There was an error processing your deletion request. Please try again later.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).WithOpenApi(op =>
        {
            op.Summary = "Delete all user data (GDPR Right to be Forgotten)";
            op.Description = "Permanently deletes all data associated with a user identified by their unique key";
            op.Parameters[0].Description = "The user's unique identification key";
            return op;
        });

        // Update user data preferences (Right to Rectification)
        app.MapPut("/data-subject/update/{key}", async (string key, [FromBody] Contact updatedInfo, [FromServices] IDb db) =>
        {
            try
            {
                using var conn = db.Connect();
                
                // First verify the key is valid
                var contact = await conn.QueryFirstOrDefaultAsync<Contact>(
                    "SELECT * FROM mail.contacts WHERE key = @key", 
                    new { key });
                
                if (contact == null)
                {
                    return Results.NotFound(new { success = false, message = "Contact not found" });
                }
                
                // Update only allowed fields (not allowing email change to prevent abuse)
                await conn.ExecuteAsync(
                    @"UPDATE mail.contacts 
                      SET name = @name
                      WHERE id = @id",
                    new { name = updatedInfo.Name, id = contact.ID });
                
                // Log this update
                await conn.ExecuteAsync(
                    @"INSERT INTO mail.data_processing_activities
                      (contact_id, activity_type, description, performed_by)
                      VALUES
                      (@contactId, 'update', 'Data subject updated their information', 'user')",
                    new { contactId = contact.ID });
                
                return Results.Ok(new 
                { 
                    success = true, 
                    message = "Your information has been updated" 
                });
            }
            catch (Exception)
            {
                return Results.Problem(
                    title: "Error updating information",
                    detail: "There was an error processing your update request. Please try again later.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }).WithOpenApi(op =>
        {
            op.Summary = "Update user data (GDPR Right to Rectification)";
            op.Description = "Updates the user's information based on their request";
            op.Parameters[0].Description = "The user's unique identification key";
            return op;
        });
    }
}
