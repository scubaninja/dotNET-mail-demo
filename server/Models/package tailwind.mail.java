package tailwind.mail.models;

import java.time.OffsetDateTime;

@Table(name = "broadcasts", schema = "mail")
public class Broadcast {
    private Integer id;
    private Integer emailId;
    private String status = "pending";
    private String name;
    private String slug;
    private String replyTo;
    private String sendToTag = "*";
    private OffsetDateTime createdAt = OffsetDateTime.now();

    private Broadcast() {
    }

    public static Broadcast fromMarkdownEmail(MarkdownEmail doc) {
        Broadcast broadcast = new Broadcast();
        broadcast.setName(doc.getData().getSubject());
        broadcast.setSlug(doc.getData().getSlug());
        broadcast.setSendToTag(doc.getData().getSendToTag());
        return broadcast;
    }

    // Getters and setters
    public Integer getId() { return id; }
    public void setId(Integer id) { this.id = id; }
    
    public Integer getEmailId() { return emailId; }
    public void setEmailId(Integer emailId) { this.emailId = emailId; }
    
    public String getStatus() { return status; }
    public void setStatus(String status) { this.status = status; }
    
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    
    public String getSlug() { return slug; }
    public void setSlug(String slug) { this.slug = slug; }
    
    public String getReplyTo() { return replyTo; }
    public void setReplyTo(String replyTo) { this.replyTo = replyTo; }
    
    public String getSendToTag() { return sendToTag; }
    public void setSendToTag(String sendToTag) { this.sendToTag = sendToTag; }
    
    public OffsetDateTime getCreatedAt() { return createdAt; }
    public void setCreatedAt(OffsetDateTime createdAt) { this.createdAt = createdAt; }
}