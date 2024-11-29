package server.Commands;

import java.sql.Connection;
import java.sql.SQLException;
import java.util.List;
import java.util.Optional;

import server.Data.CommandResult;
import server.Models.Contact;
import server.Models.Tag;

public class BulkTagCommand {
    private String tag;
    private List<String> emails;

    public BulkTagCommand(String tag, List<String> emails) {
        this.tag = tag;
        this.emails = emails;
    }

    public CommandResult execute(Connection conn) {
        int updated = 0;
        int inserted = 0;
        try {
            conn.setAutoCommit(false);
            Optional<Tag> tagOptional = getTag(conn, tag);
            Tag tag = tagOptional.orElseGet(() -> createTag(conn, tag));
            for (String email : emails) {
                Optional<Contact> contactOptional = getContact(conn, email);
                Contact contact = contactOptional.orElseGet(() -> createContact(conn, email));
                if (contactOptional.isPresent()) {
                    updated++;
                } else {
                    inserted++;
                }
                tagContact(conn, contact, tag);
            }
            conn.commit();
            return new CommandResult(inserted, updated);
        } catch (SQLException e) {
            try {
                conn.rollback();
            } catch (SQLException ex) {
                ex.printStackTrace();
            }
            return new CommandResult(e.getMessage());
        }
    }

    private Optional<Tag> getTag(Connection conn, String tagName) throws SQLException {
        // Implement the logic to get the tag from the database
        return Optional.empty();
    }

    private Tag createTag(Connection conn, String tagName) {
        // Implement the logic to create a new tag in the database
        return new Tag(tagName);
    }

    private Optional<Contact> getContact(Connection conn, String email) throws SQLException {
        // Implement the logic to get the contact from the database
        return Optional.empty();
    }

    private Contact createContact(Connection conn, String email) {
        // Implement the logic to create a new contact in the database
        return new Contact(email);
    }

    private void tagContact(Connection conn, Contact contact, Tag tag) throws SQLException {
        // Implement the logic to tag the contact in the database
    }
}
