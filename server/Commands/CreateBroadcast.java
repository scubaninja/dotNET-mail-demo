package server.Commands;

import java.sql.Connection;
import java.sql.SQLException;

import server.Data.DB;
import server.Models.Broadcast;
import server.Models.CommandResult;
import server.Models.Email;
import server.Models.MarkdownEmail;

public class CreateBroadcast {
    private Broadcast broadcast;
    private Email email;

    public CreateBroadcast(MarkdownEmail email) {
        this.email = new Email(email);
        this.broadcast = Broadcast.fromMarkdownEmail(email);
    }

    public CommandResult execute(Connection conn) throws SQLException {
        String from = System.getenv("DEFAULT_FROM");
        if (from == null || from.isEmpty()) {
            from = "noreply@tailwind.dev";
        }

        try {
            conn.setAutoCommit(false);

            // Save the email
            long emailId = DB.insert(conn, email);
            broadcast.setEmailId(emailId);
            broadcast.setReplyTo(from);

            // Create the broadcast
            long broadcastId = DB.insert(conn, broadcast);

            // Create the messages
            String sql = "INSERT INTO mail.messages (source, slug, send_to, send_from, subject, html, send_at) "
                    + "SELECT 'broadcast', ?, mail.contacts.email, ?, ?, ?, now() "
                    + "FROM mail.contacts ";

            int messagesCreated;
            if (!broadcast.getSendToTag().equals("*")) {
                sql += "INNER JOIN mail.tagged ON mail.tagged.contact_id = mail.contacts.id "
                        + "INNER JOIN mail.tags ON mail.tags.id = mail.tagged.tag_id "
                        + "WHERE subscribed = true AND mail.tags.slug = ?";

                messagesCreated = DB.executeUpdate(conn, sql, broadcastId, broadcast.getSendToTag(), email.getSlug(),
                        from, email.getSubject(), email.getHtml());
            } else {
                sql += "WHERE subscribed = true";
                messagesCreated = DB.executeUpdate(conn, sql, broadcastId, email.getSlug(), from, email.getSubject(),
                        email.getHtml());
            }

            DB.executeUpdate(conn, "NOTIFY broadcasts, ?", broadcast.getSlug());
            conn.commit();

            return new CommandResult(new Object() {
                public long BroadcastId = broadcastId;
                public long EmailId = emailId;
                public boolean Notified = true;
            }, messagesCreated, 0, 0);
        } catch (SQLException e) {
            conn.rollback();
            throw e;
        }
    }
}
