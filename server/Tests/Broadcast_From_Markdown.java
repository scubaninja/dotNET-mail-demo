package server.Tests;

import org.junit.jupiter.api.Test;
import server.Commands.CreateBroadcast;
import server.Data.DB;
import server.Models.MarkdownEmail;
import server.Models.CommandResult;

import java.sql.Connection;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;

public class Broadcast_From_Markdown {

    @Test
    public void testCreateBroadcastFromMarkdown() throws Exception {
        String markdown = "# Subject\n" +
                "Summary: This is a summary\n" +
                "SendToTag: *\n" +
                "\n" +
                "This is the body of the email.";

        MarkdownEmail email = MarkdownEmail.fromString(markdown);
        CreateBroadcast createBroadcast = new CreateBroadcast(email);

        try (Connection conn = DB.postgres()) {
            CommandResult result = createBroadcast.execute(conn);

            assertNotNull(result);
            assertEquals(1, result.getInserted());
            assertEquals(0, result.getUpdated());
        }
    }
}
