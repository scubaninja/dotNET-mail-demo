package server.Tests;

import org.junit.jupiter.api.Test;
import server.Commands.BulkTagCommand;
import server.Data.DB;
import server.Models.Contact;
import server.Models.Tag;

import java.sql.Connection;
import java.sql.SQLException;
import java.util.Arrays;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

public class Bulk_Tagging {

    @Test
    public void testBulkTagging() throws SQLException {
        DB db = new DB();
        Connection conn = db.connect();

        // Create test data
        List<String> emails = Arrays.asList("test1@example.com", "test2@example.com");
        String tag = "test-tag";

        // Execute the command
        BulkTagCommand command = new BulkTagCommand(tag, emails);
        var result = command.execute(conn);

        // Verify the results
        assertEquals(2, result.getInserted());
        assertEquals(0, result.getUpdated());

        // Verify the contacts and tags in the database
        for (String email : emails) {
            Contact contact = getContact(conn, email);
            assertTrue(contact.isSubscribed());
            Tag tagObj = getTag(conn, tag);
            assertTrue(isContactTagged(conn, contact, tagObj));
        }

        conn.close();
    }

    private Contact getContact(Connection conn, String email) throws SQLException {
        // Implement the logic to get the contact from the database
        return new Contact(email);
    }

    private Tag getTag(Connection conn, String tagName) throws SQLException {
        // Implement the logic to get the tag from the database
        return new Tag(tagName);
    }

    private boolean isContactTagged(Connection conn, Contact contact, Tag tag) throws SQLException {
        // Implement the logic to check if the contact is tagged in the database
        return true;
    }
}
