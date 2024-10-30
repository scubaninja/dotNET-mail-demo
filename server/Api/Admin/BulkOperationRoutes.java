package server.Api.Admin;

import java.sql.Connection;
import java.sql.SQLException;
import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

import server.Commands.BulkTagCommand;
import server.Data.DB;
import server.Models.CommandResult;

@RestController
public class BulkOperationRoutes {

    @Autowired
    private DB db;

    public static class BulkTagRequest {
        private boolean success = false;
        private String message;
        private String tag;
        private List<String> emails;

        // Getters and setters
    }

    public static class BulkTagResponse {
        private boolean success = false;
        private String message = "No response";
        private int created;
        private int updated;

        // Getters and setters
    }

    @PostMapping("/admin/bulk/contacts/tag")
    public ResponseEntity<BulkTagResponse> bulkTagContacts(@RequestBody BulkTagRequest request) {
        if (request.getTag() == null || request.getEmails().isEmpty()) {
            return new ResponseEntity<>(new BulkTagResponse(false, "Be sure to include a tag and at least one email address and a tag", 0, 0), HttpStatus.BAD_REQUEST);
        }

        String[] tags = request.getTag().split(",");
        int totalInserted = 0;
        int totalUpdated = 0;

        try (Connection conn = db.connect()) {
            for (String tag : tags) {
                BulkTagCommand command = new BulkTagCommand(tag.trim(), request.getEmails());
                CommandResult result = command.execute(conn);
                totalInserted += result.getInserted();
                totalUpdated += result.getUpdated();
            }

            BulkTagResponse response = new BulkTagResponse(true, tags.length + " Tag(s) applied to " + request.getEmails().size() + " contacts", totalInserted, totalUpdated);
            return new ResponseEntity<>(response, HttpStatus.OK);
        } catch (SQLException e) {
            return new ResponseEntity<>(new BulkTagResponse(false, e.getMessage(), 0, 0), HttpStatus.INTERNAL_SERVER_ERROR);
        }
    }
}
