package server.Commands;

import server.Models.Contact;
import server.Models.Activity;
import server.Data.CommandResult;
import java.sql.Connection;
import java.sql.SQLException;

public class ContactSignupCommand {
    private Contact contact;

    public ContactSignupCommand(Contact contact) {
        this.contact = contact;
    }

    public CommandResult execute(Connection conn) {
        try {
            conn.setAutoCommit(false);

            // Check if contact already exists
            String checkSql = "SELECT * FROM contacts WHERE email = ?";
            var contacts = conn.createQuery(checkSql)
                               .addParameter(contact.getEmail())
                               .executeAndFetch(Contact.class);

            if (!contacts.isEmpty()) {
                return new CommandResult(new Object() {
                    public final boolean Success = false;
                    public final String Message = "User exists";
                }, 0, 0, 0);
            }

            // Insert new contact
            String insertContactSql = "INSERT INTO contacts (name, email, subscribed, key, created_at) VALUES (?, ?, ?, ?, ?)";
            var id = conn.createUpdate(insertContactSql)
                         .addParameter(contact.getName())
                         .addParameter(contact.getEmail())
                         .addParameter(contact.isSubscribed())
                         .addParameter(contact.getKey())
                         .addParameter(contact.getCreatedAt())
                         .executeAndReturnGeneratedKey();

            // Insert activity
            String insertActivitySql = "INSERT INTO activity (contact_id, key, description, created_at) VALUES (?, ?, ?, ?)";
            conn.createUpdate(insertActivitySql)
                .addParameter(id)
                .addParameter("signup")
                .addParameter("New Contact")
                .addParameter(contact.getCreatedAt())
                .execute();

            conn.commit();

            return new CommandResult(new Object() {
                public final boolean Success = true;
                public final long ID = id;
                public final String Key = contact.getKey();
                public final boolean Subscribed = contact.isSubscribed();
            }, 1, 0, 0);

        } catch (SQLException e) {
            try {
                conn.rollback();
            } catch (SQLException ex) {
                ex.printStackTrace();
            }
            return new CommandResult(new Object() {
                public final boolean Success = false;
                public final String Message = e.getMessage();
            }, 0, 0, 0);
        }
    }
}
