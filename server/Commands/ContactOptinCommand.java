package server.Commands;

import server.Models.Contact;
import server.Data.CommandResult;
import server.Models.Activity;
import java.sql.Connection;
import java.sql.SQLException;

public class ContactOptinCommand {
    private Contact contact;

    public ContactOptinCommand(Contact contact) {
        this.contact = contact;
    }

    public CommandResult execute(Connection conn) {
        try {
            conn.setAutoCommit(false);
            contact.setSubscribed(true);
            // Assuming conn.update() and conn.insert() are implemented
            conn.update(contact);
            conn.insert(new Activity(contact.getId(), "optin", "Opted in using key"));
            conn.commit();

            return new CommandResult(new Object() {
                public final boolean Success = true;
                public final String Message = "Contact subscribed";
            }, 0, 1, 0);

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
