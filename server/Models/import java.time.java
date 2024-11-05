import java.time.OffsetDateTime;
import java.time.ZoneOffset;
import java.util.HashMap;
import java.util.Map;

public class Broadcast {
    private String status;
    private String sendToTag;
    private OffsetDateTime createdAt;
    private String name;
    private String slug;

    public Broadcast() {
        this.status = "pending";
        this.sendToTag = "*";
        this.createdAt = OffsetDateTime.now(ZoneOffset.UTC);
    }

    public static Broadcast fromMarkdownEmail(MarkdownEmail markdownEmail) {
        Broadcast broadcast = new Broadcast();
        broadcast.name = markdownEmail.getData().getSubject();
        broadcast.slug = markdownEmail.getData().getSlug();
        broadcast.sendToTag = markdownEmail.getData().getSendToTag();
        return broadcast;
    }

    public static Broadcast fromMarkdown(String markdown) {
        // Mock implementation for parsing markdown
        MarkdownEmail markdownEmail = MarkdownEmail.fromString(markdown);
        return fromMarkdownEmail(markdownEmail);
    }

    public long contactCount(DatabaseConnection dbConnection) {
        String query = sendToTag.equals("*") ? "SELECT COUNT(*) FROM Contacts" : "SELECT COUNT(*) FROM Contacts WHERE Tag = ?";
        Map<String, Object> params = new HashMap<>();
        if (!sendToTag.equals("*")) {
            params.put("Tag", sendToTag);
        }
        return dbConnection.executeScalar(query, params);
    }

    // Getters and setters
}