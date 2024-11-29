package tailwind.mail.models;

import java.time.OffsetDateTime;
import dapper.Table;

@Table(name = "activity", schema = "mail")
public class Activity {
    private Integer id;
    private Integer contactId;
    private String key;
    private String description;
    private OffsetDateTime createdAt;

    public Activity() {
        this.key = java.util.UUID.randomUUID().toString();
        this.createdAt = OffsetDateTime.now();
    }

    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public Integer getContactId() {
        return contactId;
    }

    public void setContactId(Integer contactId) {
        this.contactId = contactId;
    }

    public String getKey() {
        return key;
    }

    public void setKey(String key) {
        this.key = key;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(OffsetDateTime createdAt) {
        this.createdAt = createdAt;
    }
}
