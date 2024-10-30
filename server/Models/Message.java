package server.Models;

import java.time.OffsetDateTime;

public class Message {
    private Integer id;
    private String source = "broadcast";
    private String slug;
    private String status = "pending";
    private String sendTo;
    private String sendFrom = "noreply@tailwind.dev";
    private String subject;
    private String html;
    private OffsetDateTime sendAt = OffsetDateTime.now();
    private OffsetDateTime sentAt;
    private OffsetDateTime createdAt = OffsetDateTime.now();

    public Message(String slug, String sendTo, String subject, String html) {
        this.slug = slug;
        this.sendTo = sendTo;
        this.subject = subject;
        this.html = html;
    }

    public Message() {
    }

    public void sent() {
        this.status = "sent";
        this.sentAt = OffsetDateTime.now();
    }

    public boolean readyToSend() {
        return this.status.equals("pending") &&
                this.sendAt.isBefore(OffsetDateTime.now()) &&
                !this.sendTo.isEmpty() &&
                !this.sendFrom.isEmpty() &&
                !this.html.isEmpty() &&
                !this.subject.isEmpty();
    }

    // Getters and setters
    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public String getSource() {
        return source;
    }

    public void setSource(String source) {
        this.source = source;
    }

    public String getSlug() {
        return slug;
    }

    public void setSlug(String slug) {
        this.slug = slug;
    }

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public String getSendTo() {
        return sendTo;
    }

    public void setSendTo(String sendTo) {
        this.sendTo = sendTo;
    }

    public String getSendFrom() {
        return sendFrom;
    }

    public void setSendFrom(String sendFrom) {
        this.sendFrom = sendFrom;
    }

    public String getSubject() {
        return subject;
    }

    public void setSubject(String subject) {
        this.subject = subject;
    }

    public String getHtml() {
        return html;
    }

    public void setHtml(String html) {
        this.html = html;
    }

    public OffsetDateTime getSendAt() {
        return sendAt;
    }

    public void setSendAt(OffsetDateTime sendAt) {
        this.sendAt = sendAt;
    }

    public OffsetDateTime getSentAt() {
        return sentAt;
    }

    public void setSentAt(OffsetDateTime sentAt) {
        this.sentAt = sentAt;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(OffsetDateTime createdAt) {
        this.createdAt = createdAt;
    }
}
