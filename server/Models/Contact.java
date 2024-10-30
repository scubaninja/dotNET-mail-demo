package tailwind.mail.models;

import java.time.OffsetDateTime;
import dapper.Table;

@Table(name = "contacts", schema = "mail")
public class Contact {
    private String name;
    private String email;
    private boolean subscribed;
    private String key;
    private Integer id;
    private OffsetDateTime createdAt;

    public Contact() {
        this.key = java.util.UUID.randomUUID().toString();
    }

    public Contact(String name, String email) {
        this.name = name;
        this.email = email;
        this.key = java.util.UUID.randomUUID().toString();
    }

    // Getters and setters
    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getEmail() {
        return email;
    }

    public void setEmail(String email) {
        this.email = email;
    }

    public boolean isSubscribed() {
        return subscribed;
    }

    public void setSubscribed(boolean subscribed) {
        this.subscribed = subscribed;
    }

    public String getKey() {
        return key;
    }

    public void setKey(String key) {
        this.key = key;
    }

    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public OffsetDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(OffsetDateTime createdAt) {
        this.createdAt = createdAt;
    }
}
