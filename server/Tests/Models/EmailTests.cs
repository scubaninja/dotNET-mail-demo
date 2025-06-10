using System;
using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests.Models
{
    public class EmailTests
    {
        [Fact]
        public void Constructor_ValidMarkdownEmail_CreatesEmail()
        {
            // Arrange
            string markdown = @"---
Subject: Test Email
Summary: This is a test email
Slug: test-slug
---

# Test Content

This is the email content.";
            var markdownEmail = MarkdownEmail.FromString(markdown);

            // Act
            var email = new Email(markdownEmail);

            // Assert
            Assert.Equal("test-slug", email.Slug);
            Assert.Equal("Test Email", email.Subject);
            Assert.Equal("This is a test email", email.Preview);
            Assert.Contains("<h1>Test Content</h1>", email.Html);
            Assert.Equal(0, email.DelayHours);
            Assert.Null(email.ID);
        }

        [Fact]
        public void Constructor_NullData_ThrowsException()
        {
            // Arrange
            var markdownEmail = new MarkdownEmail(); // Has null Data

            // Act & Assert
            var exception = Assert.Throws<InvalidDataException>(() => new Email(markdownEmail));
            Assert.Contains("Markdown document should contain", exception.Message);
        }

        [Fact]
        public void Constructor_NullHtml_ThrowsException()
        {
            // Arrange
            var markdownEmail = new MarkdownEmail
            {
                Data = new System.Dynamic.ExpandoObject()
            };
            
            // Add properties to the dynamic object
            var expando = (IDictionary<string, object>)markdownEmail.Data;
            expando["Subject"] = "Test Subject";
            expando["Summary"] = "Test Summary";
            expando["Slug"] = "test-slug";

            // Act & Assert
            var exception = Assert.Throws<InvalidDataException>(() => new Email(markdownEmail));
            Assert.Contains("There should be HTML generated", exception.Message);
        }
    }
}
