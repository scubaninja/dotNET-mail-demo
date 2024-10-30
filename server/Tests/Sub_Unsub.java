package server.Tests;

import org.junit.jupiter.api.Test;
import server.Commands.ContactOptOutCommand;
import server.Commands.ContactOptinCommand;
import server.Data.CommandResult;
import server.Models.Contact;
import server.Data.DB;

import java.sql.Connection;
import java.sql.SQLException;

import static org.junit.jupiter.api.Assertions.*;

public class Sub_Unsub {

    @Test
    public void testContactOptin() throws SQLException {
        Contact contact = new Contact("John Doe", "john.doe@example.com");
        ContactOptinCommand command = new ContactOptinCommand(contact);

        try (Connection conn = new DB().connect()) {
            CommandResult result = command.execute(conn);
            assertTrue(result.getData().Success);
            assertEquals("Contact subscribed", result.getData().Message);
        }
    }

    @Test
    public void testContactOptOut() throws SQLException {
        String key = "some-unique-key";
        ContactOptOutCommand command = new ContactOptOutCommand(key);

        try (Connection conn = new DB().connect()) {
            CommandResult result = command.execute(conn);
            assertTrue(result.getData().Success);
            assertEquals("Contact unsubscribed", result.getData().Message);
        }
    }
}
