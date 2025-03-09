package tailwind.mail.models;

import java.time.OffsetDateTime;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;

public class Broadcast {
    private Integer id;
    private Integer emailId;
    private String status;
    private String name;
    private String slug;
    private String replyTo;
    private String sendToTag;
    private OffsetDateTime createdAt;

    private Broadcast() {
        this.status = "pending";
        this.sendToTag = "*";
        this.createdAt = OffsetDateTime.now();
    }

    public static Broadcast fromMarkdownEmail(MarkdownEmail doc) {
        Broadcast broadcast = new Broadcast();
        broadcast.name = doc.getData().getSubject();
        broadcast.slug = doc.getData().getSlug();
        broadcast.sendToTag = doc.getData().getSendToTag();
        return broadcast;
    }

    public static Broadcast fromMarkdown(String markdown) {
        Broadcast broadcast = new Broadcast();
        MarkdownEmail doc = MarkdownEmail.fromString(markdown);
        broadcast.name = doc.getData().getSubject();
        broadcast.slug = doc.getData().getSlug();
        broadcast.sendToTag = doc.getData().getSendToTag();
        return broadcast;
    }

    public long contactCount(Connection conn) throws SQLException {
        long contacts = 0;
        if (sendToTag.equals("*")) {
            String sql = "SELECT COUNT(1) FROM mail.contacts WHERE subscribed = true";
            try (PreparedStatement stmt = conn.prepareStatement(sql);
                 ResultSet rs = stmt.executeQuery()) {
                if (rs.next()) {
                    contacts = rs.getLong(1);
                }
            }
        } else {
            String sql = "SELECT COUNT(1) AS count FROM mail.contacts " +
                    "INNER JOIN mail.tagged ON mail.tagged.contact_id = mail.contacts.id " +
                    "INNER JOIN mail.tags ON mail.tags.id = mail.tagged.tag_id " +
                    "WHERE subscribed = true AND tags.slug = ?";
            try (PreparedStatement stmt = conn.prepareStatement(sql)) {
                stmt.setString(1, sendToTag);
                try (ResultSet rs = stmt.executeQuery()) {
                    if (rs.next()) {
                        contacts = rs.getLong(1);
                    }
                }
            }
        }
        return contacts;
    }

    // Getters and setters
    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public Integer getEmailId() {
        return emailId;
    }

    public void setEmailId(Integer emailId) {
        this.emailId = emailId;
    }

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getSlug() {
        return slug;
    }

    public void setSlug(String slug) {
        this.slug = slug;
    }

    public String getReplyTo() {
        return replyTo;
    }

    public void setReplyTo(String replyTo) {
        this.replyTo = replyTo;
    }

    public String getSendToTag() {
        return sendToTag;
    }

    public void setSendToTag(String sendToTag) {
        this.sendToTag = sendToTag;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(OffsetDateTime createdAt) {
        this.createdAt = createdAt;
    }
}
