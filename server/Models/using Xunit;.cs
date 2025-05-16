using Xunit;
using Moq;
using System.Data;
using Dapper;
using System;

namespace Tailwind.Mail.Models.Tests
{
    /// <summary>
    /// Test suite for the Broadcast model class
    /// </summary>
    public class BroadcastTests
    {
        // Mock database connection used across test methods
        private Mock<IDbConnection> mockConnection;

        /// <summary>
        /// Constructor initializes a fresh mock database connection for each test
        /// </summary>
        public BroadcastTests()
        {
            mockConnection = new Mock<IDbConnection>();
        }

        /// <summary>
        /// Tests that a Broadcast can be created successfully from a valid MarkdownEmail
        /// Verifies all properties are correctly mapped from the source document
        /// </summary>
        [Fact]
        public void FromMarkdownEmail_ValidInput_CreatesBroadcast()
        {
            // Arrange - Create test data with known values
            var mockData = new MarkdownEmailData
            {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            };
            var mockDoc = new MarkdownEmail { Data = mockData };

            // Act - Create broadcast from test data
            var broadcast = Broadcast.FromMarkdownEmail(mockDoc);

            // Assert - Verify all properties match input data
            Assert.NotNull(broadcast);
            Assert.Equal("Test Subject", broadcast.Name);
            Assert.Equal("test-slug", broadcast.Slug);
            Assert.Equal("test-tag", broadcast.SendToTag);
        }

        /// <summary>
        /// Verifies that attempting to create a Broadcast from a null MarkdownEmail
        /// throws the appropriate exception
        /// </summary>
        [Fact]
        public void FromMarkdownEmail_NullDocument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(null));
        }

        /// <summary>
        /// Tests that FromMarkdown method properly validates its input string
        /// Checks null, empty, and whitespace cases
        /// </summary>
        [Fact]
        public void FromMarkdown_NullOrEmptyInput_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(string.Empty));
            Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(null));
            Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown("   "));
        }

        /// <summary>
        /// Tests that ContactCount returns correct number when querying all subscribed contacts
        /// Uses wildcard SendToTag "*" which should count all subscribed contacts
        /// </summary>
        [Fact]
        public void ContactCount_AllContacts_ReturnsCorrectCount()
        {
            // Arrange - Setup mock to return 100 contacts when querying all subscribers
            mockConnection.Setup(c => c.ExecuteScalar<long>(
                It.Is<string>(sql => sql.Contains("WHERE subscribed = true")),
                null, null, null, null))
                .Returns(100);

            var broadcast = Broadcast.FromMarkdownEmail(new MarkdownEmail 
            { 
                Data = new MarkdownEmailData { SendToTag = "*" } 
            });

            // Act - Get contact count
            var count = broadcast.ContactCount(mockConnection.Object);

            // Assert - Verify expected count
            Assert.Equal(100, count);
        }

        /// <summary>
        /// Tests that ContactCount returns correct number when filtering by a specific tag
        /// Verifies the SQL joins and tag filtering work correctly
        /// </summary>
        [Fact]
        public void ContactCount_SpecificTag_ReturnsCorrectCount()
        {
            // Arrange - Setup mock to return 50 contacts when querying specific tag
            mockConnection.Setup(c => c.ExecuteScalar<long>(
                It.Is<string>(sql => sql.Contains("INNER JOIN mail.tagged")),
                It.Is<object>(param => ((dynamic)param).tagSlug == "test-tag"),
                null, null, null))
                .Returns(50);

            var broadcast = Broadcast.FromMarkdownEmail(new MarkdownEmail 
            { 
                Data = new MarkdownEmailData { SendToTag =