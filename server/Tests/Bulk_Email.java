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

public class Bulk_Email {

    @Test
    public void testBulkTagCommand() throws SQLException {
        DB db = new DB();
        Connection conn = db.Connect();

        List<String> emails = Arrays.asList("test1@example.com", "test2@example.com");
        BulkTagCommand command = new BulkTagCommand("test-tag", emails);

        var result = command.execute(conn);

        assertTrue(result.getInserted() > 0 || result.getUpdated() > 0);
        assertEquals("test-tag", getTag(conn, "test-tag").get().getName());
        for (String email : emails) {
            assertTrue(getContact(conn, email).isPresent());
        }

        conn.close();
    }

    private Optional<Tag> getTag(Connection conn, String tagName) throws SQLException {
        // Implement the logic to get the tag from the database
        return Optional.empty();
    }

    private Optional<Contact> getContact(Connection conn, String email) throws SQLException {
        // Implement the logic to get the contact from the database
        return Optional.empty();
    }
}
