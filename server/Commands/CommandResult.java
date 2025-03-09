package server.Commands;

public class CommandResult {
    private Object data;
    private int inserted;
    private int updated;
    private int deleted;

    public CommandResult() {
    }

    public CommandResult(Object data, int inserted, int updated, int deleted) {
        this.data = data;
        this.inserted = inserted;
        this.updated = updated;
        this.deleted = deleted;
    }

    public Object getData() {
        return data;
    }

    public void setData(Object data) {
        this.data = data;
    }

    public int getInserted() {
        return inserted;
    }

    public void setInserted(int inserted) {
        this.inserted = inserted;
    }

    public int getUpdated() {
        return updated;
    }

    public void setUpdated(int updated) {
        this.updated = updated;
    }

    public int getDeleted() {
        return deleted;
    }

    public void setDeleted(int deleted) {
        this.deleted = deleted;
    }
}
