using System;
using System.IO;
using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests.Models
{
    public class MarkdownEmailTests
    {
        [Fact]
        public void FromString_ValidMarkdown_ReturnsValidObject()
        {
            // Arrange
            string markdown = @"---
Subject: Test Email
Summary: This is a test email
---

# Hello World

This is a test email with markdown content.";

            // Act
            var email = MarkdownEmail.FromString(markdown);

            // Assert
            Assert.NotNull(email);
            Assert.NotNull(email.Data);
            Assert.NotNull(email.Html);
            Assert.Equal("Test Email", email.Data.Subject);
            Assert.Equal("This is a test email", email.Data.Summary);
            Assert.Contains("<h1>Hello World</h1>", email.Html);
        }

        [Fact]
        public void FromString_MissingRequiredFields_IsNotValid()
        {
            // Arrange
            string markdown = @"---
Title: Just a title without required fields
---

# Content without required fields";

            // Act
            var email = MarkdownEmail.FromString(markdown);

            // Assert
            Assert.NotNull(email);
            Assert.False(email.IsValid());
        }

        [Fact]
        public void Render_NullMarkdown_ThrowsException()
        {
            // Arrange
            var email = new MarkdownEmail();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => email.GetType()
                .GetMethod("Render", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(email, null));

            Assert.Contains("Markdown is null", exception.InnerException.Message);
        }

        [Fact]
        public void FromString_DefaultValuesAreSet()
        {
            // Arrange
            string markdown = @"---
Subject: Test Email
Summary: This is a test email
---

Content";

            // Act
            var email = MarkdownEmail.FromString(markdown);

            // Assert
            Assert.NotNull(email.Data);
            Assert.Equal("test-email", email.Data.Slug);
            Assert.Equal("*", email.Data.SendToTag);
        }

        [Fact]
        public void FromString_CustomValuesAreRespected()
        {
            // Arrange
            string markdown = @"---
Subject: Test Email
Summary: This is a test email
Slug: custom-slug
SendToTag: premium-users
---

Content";

            // Act
            var email = MarkdownEmail.FromString(markdown);

            // Assert
            Assert.NotNull(email.Data);
            Assert.Equal("custom-slug", email.Data.Slug);
            Assert.Equal("premium-users", email.Data.SendToTag);
        }
    }
}
