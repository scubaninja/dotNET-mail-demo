public class Broadcast {
    private static final String DEFAULT_STATUS = "pending";
    private static final String DEFAULT_SEND_TO_TAG = "*";

    private String status;
    private String sendToTag;
    private OffsetDateTime createdAt;
    private String name;
    private String slug;

    public Broadcast() {
        this.status = DEFAULT_STATUS;
        this.sendToTag = DEFAULT_SEND_TO_TAG;
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

    // Getters and setters
}