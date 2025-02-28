package com.tailwind.mail.controllers;

import com.tailwind.mail.commands.ContactOptOutCommand;
import com.tailwind.mail.commands.LinkClickedCommand;
import com.tailwind.mail.models.Contact;
import com.tailwind.mail.models.SignUpRequest;
import com.tailwind.mail.repositories.ContactRepository;
import io.swagger.v3.oas.annotations.Operation;
import org.springframework.web.bind.annotation.*;

@RestController
public class PublicController {

    private final ContactRepository contactRepository;

    public PublicController(ContactRepository contactRepository) {
        this.contactRepository = contactRepository;
    }

    @GetMapping("/about")
    @Operation(summary = "Information about the API")
    public String about() {
        return "Tailwind Traders Mail Services API";
    }

    @GetMapping("/unsubscribe/{key}")
    @Operation(summary = "Unsubscribe from the mailing list")
    public boolean unsubscribe(@PathVariable String key) {
        return new ContactOptOutCommand(key).execute();
    }

    @GetMapping("/link/clicked/{key}")
    @Operation(summary = "Track a link click")
    public boolean linkClicked(@PathVariable String key) {
        return new LinkClickedCommand(key).execute();
    }

    @PostMapping("/signup")
    @Operation(summary = "Sign up for the mailing list")
    public Contact signup(@RequestBody SignUpRequest request) {
        Contact contact = new Contact();
        contact.setEmail(request.getEmail());
        contact.setName(request.getName());
        return contactRepository.save(contact);
    }
}
