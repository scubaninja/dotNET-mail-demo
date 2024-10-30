package tailwind.mail.tests;

import org.junit.jupiter.api.Test;
import tailwind.ai.Chat;

import static org.junit.jupiter.api.Assertions.assertNotNull;

public class AITest {

    @Test
    public void testSimpleApiPing() throws Exception {
        Chat chat = new Chat();
        String res = chat.prompt("Create a 4-paragraph essay on the topic of 'How much gold is still missing from the Spanish sunken fleet'.");
        System.out.println(res);
        assertNotNull(res);
    }
}
