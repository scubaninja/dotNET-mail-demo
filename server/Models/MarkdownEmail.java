package server.Models;

import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.UUID;

import org.commonmark.node.Node;
import org.commonmark.parser.Parser;
import org.commonmark.renderer.html.HtmlRenderer;
import org.yaml.snakeyaml.DumperOptions;
import org.yaml.snakeyaml.Yaml;
import org.yaml.snakeyaml.constructor.Constructor;
import org.yaml.snakeyaml.nodes.Tag;
import org.yaml.snakeyaml.representer.Representer;

public class MarkdownEmail {
    private String markdown;
    private String html;
    private Map<String, Object> data;
    private List<String> tags;

    public MarkdownEmail() {
        this.tags = new ArrayList<>();
    }

    public static MarkdownEmail fromFile(String path) throws IOException {
        MarkdownEmail email = new MarkdownEmail();
        email.markdown = readFile(path);
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
        if (data == null) {
            return false;
        }
        return data.containsKey("Subject") && data.containsKey("Summary");
    }

    private void render() {
        if (markdown == null) {
            throw new IllegalStateException("Markdown is null; be sure to set that first");
        }

        Parser parser = Parser.builder().build();
        Node document = parser.parse(markdown);
        HtmlRenderer renderer = HtmlRenderer.builder().build();
        html = renderer.render(document);

        Yaml yaml = new Yaml(new Constructor(), new Representer(), new DumperOptions(), new Tag("!"));
        data = yaml.load(new StringReader(markdown));

        if (!data.containsKey("Slug")) {
            data.put("Slug", ((String) data.get("Subject")).toLowerCase().replace(" ", "-"));
        }
        if (!data.containsKey("SendToTag")) {
            data.put("SendToTag", "*");
        }
    }

    private static String readFile(String path) throws IOException {
        StringBuilder content = new StringBuilder();
        try (FileReader reader = new FileReader(new File(path))) {
            char[] buffer = new char[1024];
            int bytesRead;
            while ((bytesRead = reader.read(buffer)) != -1) {
                content.append(buffer, 0, bytesRead);
            }
        }
        return content.toString();
    }

    // Getters and setters
    public String getMarkdown() {
        return markdown;
    }

    public void setMarkdown(String markdown) {
        this.markdown = markdown;
    }

    public String getHtml() {
        return html;
    }

    public void setHtml(String html) {
        this.html = html;
    }

    public Map<String, Object> getData() {
        return data;
    }

    public void setData(Map<String, Object> data) {
        this.data = data;
    }

    public List<String> getTags() {
        return tags;
    }

    public void setTags(List<String> tags) {
        this.tags = tags;
    }
}
