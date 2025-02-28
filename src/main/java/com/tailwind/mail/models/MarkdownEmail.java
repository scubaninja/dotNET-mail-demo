package com.tailwind.mail.models;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.commonmark.parser.Parser;
import org.commonmark.renderer.html.HtmlRenderer;
import org.yaml.snakeyaml.Yaml;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

public class MarkdownEmail {
    private String markdown;
    private String html;
    private Map<String, Object> data;
    private List<String> tags = new ArrayList<>();

    public static MarkdownEmail fromFile(String path) throws IOException {
        MarkdownEmail email = new MarkdownEmail();
        email.markdown = Files.readString(new File(path).toPath());
        email.render();
        return email;
    }

    public static MarkdownEmail fromString(String markdown) {
        MarkdownEmail email = new MarkdownEmail();
        email.markdown = markdown;
        email.render();
        return email;
    }

    public boolean isValid() {
        return data != null && data.containsKey("subject") && data.containsKey("summary");
    }

    private void render() {
        if (markdown == null) {
            throw new IllegalStateException("Markdown is null; be sure to set that first");
        }

        // Convert markdown to HTML
        Parser parser = Parser.builder().build();
        HtmlRenderer renderer = HtmlRenderer.builder().build();
        html = renderer.render(parser.parse(markdown));

        // Parse YAML front matter
        Yaml yaml = new Yaml();
        String[] parts = markdown.split("---", 3);
        if (parts.length >= 2) {
            data = yaml.load(parts[1]);
            if (!data.containsKey("slug")) {
                data.put("slug", ((String) data.get("subject")).toLowerCase().replace(" ", "-"));
            }
            if (!data.containsKey("sendToTag")) {
                data.put("sendToTag", "*");
            }
        }
    }

    // Getters and setters
    // ...existing code...
}
