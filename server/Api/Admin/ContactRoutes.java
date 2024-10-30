package server.Api.Admin;

import java.sql.Connection;
import java.sql.SQLException;
import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import server.Data.DB;
import server.Models.Contact;

@RestController
public class ContactRoutes {

    @Autowired
    private DB db;

    public static class ContactSearchResponse {
        private String term;
        private List<Contact> contacts;

        // Getters and setters
    }

    @GetMapping("/admin/contacts/search")
    public ResponseEntity<ContactSearchResponse> searchContacts(@RequestParam String term) {
        ContactSearchResponse response = new ContactSearchResponse();
        response.setTerm(term);

        String sql = "SELECT * FROM mail.contacts WHERE email ~* ? OR name ~* ?";
        try (Connection conn = db.connect()) {
            List<Contact> contacts = conn.createQuery(sql)
                                          .addParameter(term)
                                          .addParameter(term)
                                          .executeAndFetch(Contact.class);
            response.setContacts(contacts);
            return new ResponseEntity<>(response, HttpStatus.OK);
        } catch (SQLException e) {
            return new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR);
        }
    }
}
