using Xunit;
using System;
using System.Data;
using Tailwind.Mail.Models;
using Tailwind.Data;
using Npgsql;

namespace Tailwind.Mail.Tests
{
    public class BroadcastTests : IDisposable
    {
        private readonly IDbConnection _db;

        public BroadcastTests()
        {
            // Setup test database connection
            _db = new DB().Connect();
            SetupTestData();
        }

        private void SetupTestData()
        {
            // Create test schema and tables
            _db.Execute(@"
                CREATE SCHEMA IF NOT EXISTS mail;
                CREATE TABLE IF NOT EXISTS mail.broadcasts (
                    id SERIAL PRIMARY KEY,
                    email_id INTEGER,
                    status VARCHAR(50),
                    name VARCHAR(255),
                    slug VARCHAR(255),
                    reply_to VARCHAR(255),
                    send_to_tag VARCHAR(255),
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                );
            ");
        }

        [Fact]
        public void Broadcast_WhenCreated_HasDefaultStatus()
        {
            // Arrange
            var broadcast = new Broadcast();

            // Assert
            Assert.Equal("pending", broadcast.Status);
        }

        [Fact]
        public void FromMarkdownEmail_WithValidInput_CreatesBroadcast()
        {
            // Arrange
            var doc = new MarkdownEmail
            {
                Data = new { 
                    Title = "Test Email",
                    Slug = "test-email",
                    SendToTag = "test-tag",
                    ReplyTo = "test@example.com"
                }
            };

            // Act
            var broadcast = Broadcast.FromMarkdownEmail(doc);

            // Assert 
            Assert.Equal("Test Email", broadcast.Name);
            Assert.Equal("test-email", broadcast.Slug);
            Assert.Equal("test-tag", broadcast.SendToTag);
            Assert.Equal("test@example.com", broadcast.ReplyTo);
            Assert.Equal("draft", broadcast.Status);
        }

        [Fact]
        public void FromMarkdownEmail_WithNullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(null));
        }

        [Fact]
        public void ContactCount_WithValidTag_ReturnsCorrectCount()
        {
            // Arrange
            var broadcast = new Broadcast { SendToTag = "test-tag" };
            
            // Setup test contacts data
            _db.Execute(@"
                CREATE TABLE IF NOT EXISTS mail.contacts (
                    id SERIAL PRIMARY KEY,
                    subscribed BOOLEAN DEFAULT true
                );
                CREATE TABLE IF NOT EXISTS mail.tags (
                    id SERIAL PRIMARY KEY,
                    slug VARCHAR(255)
                );
                CREATE TABLE IF NOT EXISTS mail.tagged (
                    contact_id INTEGER,
                    tag_id INTEGER
                );
                INSERT INTO mail.contacts (subscribed) VALUES (true);
                INSERT INTO mail.tags (slug) VALUES ('test-tag');
                INSERT INTO mail.tagged (contact_id, tag_id) VALUES (1, 1);
            ");

            // Act
            var count = broadcast.ContactCount(_db);

            // Assert
            Assert.Equal(1, count);
        }

        public void Dispose()
        {
            // Cleanup test data
            _db.Execute(@"
                DROP TABLE IF EXISTS mail.tagged;
                DROP TABLE IF EXISTS mail.tags;
                DROP TABLE IF EXISTS mail.contacts;
                DROP TABLE IF EXISTS mail.broadcasts;
                DROP SCHEMA IF EXISTS mail;
            ");
            _db.Dispose();
        }
    }
}