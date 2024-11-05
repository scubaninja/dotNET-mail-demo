import org.junit.jupiter.api.Test;
import org.mockito.Mockito;

import java.time.OffsetDateTime;
import java.time.ZoneOffset;
import java.util.HashMap;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;

public class BroadcastTest {

    @Test
    public void broadcastInitialization() {
        Broadcast broadcast = new Broadcast();
        assertNotNull(broadcast);
        assertEquals("pending", broadcast.getStatus());
        assertEquals("*", broadcast.getSendToTag());
        assertEquals(OffsetDateTime.now(ZoneOffset.UTC).toLocalDate(), broadcast.getCreatedAt().toLocalDate());
    }

    @Test
    public void broadcastFromMarkdownEmail() {
        MarkdownEmail markdownEmail = new MarkdownEmail();
        EmailData emailData = new EmailData();
        emailData.setSubject("Test Subject");
        emailData.setSlug("test-slug");
        emailData.setSendToTag("test-tag");
        markdownEmail.setData(emailData);

        Broadcast broadcast = Broadcast.fromMarkdownEmail(markdownEmail);

        assertEquals("Test Subject", broadcast.getName());
        assertEquals("test-slug", broadcast.getSlug());
        assertEquals("test-tag", broadcast.getSendToTag());
    }

    @Test
    public void broadcastFromMarkdown() {
        String markdown = "---\nsubject: Test Subject\nslug: test-slug\nsendToTag: test-tag\n---";

        Broadcast broadcast = Broadcast.fromMarkdown(markdown);

        assertEquals("Test Subject", broadcast.getName());
        assertEquals("test-slug", broadcast.getSlug());
        assertEquals("test-tag", broadcast.getSendToTag());
    }

    @Test
    public void broadcastContactCountWithTag() {
        DatabaseConnection mockDbConnection = Mockito.mock(DatabaseConnection.class);
        Mockito.when(mockDbConnection.executeScalar(Mockito.anyString(), Mockito.anyMap()))
                .thenReturn(10L);

        Broadcast broadcast = new Broadcast();
        broadcast.setSendToTag("test-tag");

        long count = broadcast.contactCount(mockDbConnection);

        assertEquals(10, count);
    }

    @Test
    public void broadcastContactCountWithoutTag() {
        DatabaseConnection mockDbConnection = Mockito.mock(DatabaseConnection.class);
        Mockito.when(mockDbConnection.executeScalar(Mockito.anyString(), Mockito.anyMap()))
                .thenReturn(100L);

        Broadcast broadcast = new Broadcast();
        broadcast.setSendToTag("*");

        long count = broadcast.contactCount(mockDbConnection);