package com.tailwind.mail.controllers;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.tailwind.mail.models.SignUpRequest;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

@SpringBootTest
@AutoConfigureMockMvc
public class PublicControllerTests {

    @Autowired
    private MockMvc mockMvc;

    @Autowired
    private ObjectMapper objectMapper;

    @Test
    public void testAbout() throws Exception {
        mockMvc.perform(get("/about"))
               .andExpect(status().isOk())
               .andExpect(content().string("Tailwind Traders Mail Services API"));
    }

    @Test
    public void testSignup() throws Exception {
        SignUpRequest request = new SignUpRequest();
        request.setEmail("test@example.com");
        request.setName("Test User");

        mockMvc.perform(post("/signup")
               .contentType(MediaType.APPLICATION_JSON)
               .content(objectMapper.writeValueAsString(request)))
               .andExpect(status().isOk())
               .andExpect(jsonPath("$.email").value("test@example.com"))
               .andExpect(jsonPath("$.name").value("Test User"));
    }

    @Test
    public void testUnsubscribe() throws Exception {
        mockMvc.perform(get("/unsubscribe/test-key"))
               .andExpect(status().isOk());
    }

    @Test
    public void testLinkClicked() throws Exception {
        mockMvc.perform(get("/link/clicked/test-key"))
               .andExpect(status().isOk());
    }
}
