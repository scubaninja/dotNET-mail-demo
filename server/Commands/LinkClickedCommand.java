package server.Commands;

public class LinkClickedCommand {
    private String key;

    public LinkClickedCommand(String key) {
        this.key = key;
    }

    public String getKey() {
        return key;
    }

    public void setKey(String key) {
        this.key = key;
    }

    public String execute() {
        return "Link clicked: " + key;
    }
}
