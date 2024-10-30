package server.Models;

import java.time.OffsetDateTime;

public class Email {
    private Integer id;
    private String slug;
    private String subject;
    private String preview;
    private int delayHours = 0;
    private String html;
    private OffsetDateTime createdAt = OffsetDateTime.now();

    public Email(MarkdownEmail doc) {
        if (doc.getData() == null) {
            throw new IllegalArgumentException("Markdown document should contain Slug, Subject, and Summary at least");
        }
        if (doc.getHtml() == null) {
            throw new IllegalArgumentException("There should be HTML generated from the markdown document");
        }
        this.slug = doc.getData().getSlug();
        this.subject = doc.getData().getSubject();
        this.preview = doc.getData().getSummary();
        this.html = doc.getHtml();
    }

    // Getters and setters
    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public String getSlug() {
        return slug;
    }

    public void setSlug(String slug) {
        this.slug = slug;
    }

    public String getSubject() {
        return subject;
    }

    public void setSubject(String subject) {
        this.subject = subject;
    }

    public String getPreview() {
        return preview;
    }

    public void setPreview(String preview) {
        this.preview = preview;
    }

    public int getDelayHours() {
        return delayHours;
    }

    public void setDelayHours(int delayHours) {
        this.delayHours = delayHours;
    }

    public String getHtml() {
        return html;
    }

    public void setHtml(String html) {
        this.html = html;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(OffsetDateTime createdAt) {
        this.createdAt = createdAt;
    }
}
