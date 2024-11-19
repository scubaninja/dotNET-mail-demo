using Tailwind.Data;
using Dapper;
using System.Data;

namespace Tailwind.Mail.Models
{
    // The process of creating a broadcast is:
    // The initial data is created first (name, slug)
    // Then the email and finally the segment to send to, which is done by tag (or not)
    // If the initial use case is using a markdown document, then it should contain all 
    // that we need

    // Map this class to the "broadcasts" table in the "mail" schema
    [Table("broadcasts", Schema = "mail")]
    public class Broadcast
    {
        // Nullable integer property for the broadcast ID
        public int? ID { get; set; }

        // Nullable integer property for the associated email ID
        public int? EmailId { get; set; }

        // String property for the status of the broadcast, default value is "pending"
        public string Status { get; set; } = "pending";

        // Nullable string property for the name of the broadcast
        public string? Name { get; set; }

        // DateTimeOffset property for the creation date and time
        public DateTimeOffset CreatedAt { get; set; }

        // Private constructor to prevent direct instantiation
        private Broadcast()
        {
            // Set the CreatedAt property to the current UTC time
            CreatedAt = DateTimeOffset.UtcNow;
        }

        // Static method to create a Broadcast instance from a MarkdownEmail document
        public static Broadcast FromMarkdownEmail(MarkdownEmail doc)
        {
            // Create a new Broadcast instance
            var broadcast = new Broadcast();

            // Set the Name property using the subject from the MarkdownEmail document
            broadcast.Name = doc.Data.Subject;

            // Return the created Broadcast instance
            return broadcast;
        }
    }
}