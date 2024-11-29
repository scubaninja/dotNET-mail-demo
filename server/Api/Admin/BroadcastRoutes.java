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

import server.Commands.CreateBroadcast;
import server.Data.DB;
import server.Models.MarkdownEmail;
import server.Models.Broadcast;
import server.Models.CommandResult;

@RestController
public class BroadcastRoutes {

    @Autowired
    private DB db;

    public static class ValidationResponse {
        private boolean valid;
        private String message;
        private long contacts;
        private MarkdownEmail data;

        public ValidationResponse() {
            this.message = "The markdown is valid";
        }

        // Getters and setters
    }

    public static class ValidationRequest {
        private String markdown;

        // Getters and setters
    }

    public static class ChatRequest {
        private String prompt;

        // Getters and setters
    }

    public static class ChatResponse {
        private boolean success;
        private String prompt;
        private String reply;

        // Getters and setters
    }

    public static class QueueBroadcastResponse {
        private boolean success;
        private String message;
        private CommandResult result;

        // Getters and setters
    }

    @PostMapping("/admin/get-chat")
    public ResponseEntity<ChatResponse> getChat(@RequestBody ChatRequest req) {
        if (req.getPrompt() == null) {
            return new ResponseEntity<>(new ChatResponse(false, req.getPrompt(), "Ensure there is a Subject and Prompt in the request"), HttpStatus.BAD_REQUEST);
        } else {
            Chat chat = new Chat();
            String res = chat.prompt(req.getPrompt());
            return new ResponseEntity<>(new ChatResponse(true, req.getPrompt(), res), HttpStatus.OK);
        }
    }

    @PostMapping("/admin/queue-broadcast")
    public ResponseEntity<QueueBroadcastResponse> queueBroadcast(@RequestBody ValidationRequest req) {
        String markdown = req.getMarkdown();
        MarkdownEmail doc = MarkdownEmail.fromString(markdown);

        if (!doc.isValid()) {
            return new ResponseEntity<>(new QueueBroadcastResponse(false, "Ensure there is a Body, Subject and Summary in the markdown", null), HttpStatus.BAD_REQUEST);
        }

        try (Connection conn = db.connect()) {
            CommandResult res = new CreateBroadcast(doc).execute(conn);
            QueueBroadcastResponse response = new QueueBroadcastResponse(true, "The broadcast was queued with ID " + res.getData().getBroadcastId() + " and " + res.getInserted() + " messages were created", res);
            return new ResponseEntity<>(response, HttpStatus.OK);
        } catch (SQLException e) {
            return new ResponseEntity<>(new QueueBroadcastResponse(false, e.getMessage(), null), HttpStatus.INTERNAL_SERVERError);
        }
    }

    @PostMapping("/admin/validate")
    public ResponseEntity<ValidationResponse> validate(@RequestBody ValidationRequest req) {
        if (req.getMarkdown() == null) {
            return new ResponseEntity<>(new ValidationResponse(false, "The markdown is null", 0, null), HttpStatus.BAD_REQUEST);
        }

        MarkdownEmail doc = MarkdownEmail.fromString(req.getMarkdown());
        if (!doc.isValid()) {
            return new ResponseEntity<>(new ValidationResponse(false, "Ensure there is a Subject and Summary in the markdown", 0, doc), HttpStatus.BAD_REQUEST);
        }

        Broadcast broadcast = Broadcast.fromMarkdownEmail(doc);
        try (Connection conn = db.connect()) {
            long contacts = broadcast.contactCount(conn);
            ValidationResponse response = new ValidationResponse(true, "The markdown is valid", contacts, doc);
            return new ResponseEntity<>(response, HttpStatus.OK);
        } catch (SQLException e) {
            return new ResponseEntity<>(new ValidationResponse(false, e.getMessage(), 0, null), HttpStatus.INTERNAL_SERVERError);
        }
    }
}
