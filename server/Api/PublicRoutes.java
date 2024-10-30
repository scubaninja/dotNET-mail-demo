package server.Api;

import java.sql.Connection;
import java.sql.SQLException;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

import server.Commands.ContactOptOutCommand;
import server.Commands.LinkClickedCommand;
import server.Data.DB;
import server.Models.Contact;
import server.Models.SignUpRequest;

@RestController
public class PublicRoutes {

    @Autowired
    private DB db;

    @GetMapping("/about")
    public ResponseEntity<String> about() {
        return new ResponseEntity<>("Tailwind Traders Mail Services API", HttpStatus.OK);
    }

    @GetMapping("/unsubscribe/{key}")
    public ResponseEntity<Boolean> unsubscribe(@PathVariable String key) {
        try (Connection conn = db.connect()) {
            ContactOptOutCommand cmd = new ContactOptOutCommand(key);
            boolean result = cmd.execute(conn).getUpdated() > 0;
            return new ResponseEntity<>(result, HttpStatus.OK);
        } catch (SQLException e) {
            return new ResponseEntity<>(false, HttpStatus.INTERNAL_SERVER_ERROR);
        }
    }

    @GetMapping("/link/clicked/{key}")
    public ResponseEntity<String> linkClicked(@PathVariable String key) {
        LinkClickedCommand cmd = new LinkClickedCommand(key);
        String result = cmd.execute();
        return new ResponseEntity<>(result, HttpStatus.OK);
    }

    @PostMapping("/signup")
    public ResponseEntity<Integer> signUp(@RequestBody SignUpRequest req) {
        Contact contact = new Contact(req.getName(), req.getEmail());
        try (Connection conn = db.connect()) {
            int result = conn.createQuery("INSERT INTO contacts (email, name) VALUES (?, ?)")
                             .addParameter(contact.getEmail())
                             .addParameter(contact.getName())
                             .executeUpdate();
            return new ResponseEntity<>(result, HttpStatus.OK);
        } catch (SQLException e) {
            return new ResponseEntity<>(0, HttpStatus.INTERNAL_SERVER_ERROR);
        }
    }
}
