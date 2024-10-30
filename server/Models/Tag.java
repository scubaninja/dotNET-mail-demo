package server.Models;

public class Tag {
    private Integer id;
    private String slug;
    private String name;
    private String description;

    public Tag(String name) {
        this.name = name;
        this.slug = name.toLowerCase().replace(" ", "-");
    }

    public Tag() {
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

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }
}

public class Tagged {
    private Integer contactId;
    private Integer tagId;

    // Getters and setters
    public Integer getContactId() {
        return contactId;
    }

    public void setContactId(Integer contactId) {
        this.contactId = contactId;
    }

    public Integer getTagId() {
        return tagId;
    }

    public void setTagId(Integer tagId) {
        this.tagId = tagId;
    }
}
