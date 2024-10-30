package server.Commands;

import server.Models.Contact;
import server.Data.CommandResult;
import server.Models.Activity;
import java.sql.Connection;
import java.sql.SQLException;
import java.util.List;

public class ContactOptOutCommand {
    private String key;

    public ContactOptOutCommand(String key) {
        this.key = key;
    }

    public CommandResult execute(Connection conn) {
        try {
            conn.setAutoCommit(false);
            List<Contact> contacts = conn.createQuery("SELECT * FROM contacts WHERE key = ?")
                                          .addParameter(key)
                                          .executeAndFetch(Contact.class);
            if (contacts.isEmpty()) {
                return new CommandResult(new Object() {
                    public final boolean Success = false;
                    public final String Message = "Contact not found";
                }, 0, 0, 0);
            }

            Contact contact = contacts.get(0);
            contact.setSubscribed(false);
            conn.update(contact);
            conn.insert(new Activity(contact.getId(), "optout", "Unsubbed"));
            conn.commit();

            return new CommandResult(new Object() {
                public final boolean Success = true;
                public final String Message = "Contact unsubscribed";
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
