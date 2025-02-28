import unittest
from datetime import datetime
from server.models.broadcast import Broadcast
import sqlite3

class TestBroadcast(unittest.TestCase):

    def test_from_markdown_email(self):
        class MockMarkdownEmail:
            class Data:
                subject = "Test Subject"
                slug = "test-slug"
                send_to_tag = "test-tag"
            data = Data()

        doc = MockMarkdownEmail()
        broadcast = Broadcast.from_markdown_email(doc)
        self.assertEqual(broadcast.name, "Test Subject")
        self.assertEqual(broadcast.slug, "test-slug")
        self.assertEqual(broadcast.send_to_tag, "test-tag")

    def test_from_markdown(self):
        class MockMarkdownEmail:
            class Data:
                subject = "Test Subject"
                slug = "test-slug"
                send_to_tag = "test-tag"
            data = Data()

            @staticmethod
            def from_string(markdown):
                return MockMarkdownEmail()

        markdown = "Some markdown content"
        Broadcast.MarkdownEmail = MockMarkdownEmail
        broadcast = Broadcast.from_markdown(markdown)
        self.assertEqual(broadcast.name, "Test Subject")
        self.assertEqual(broadcast.slug, "test-slug")
        self.assertEqual(broadcast.send_to_tag, "test-tag")

    def test_contact_count(self):
        conn = sqlite3.connect(":memory:")
        conn.execute("CREATE TABLE contacts (id INTEGER PRIMARY KEY, subscribed BOOLEAN)")
        conn.execute("CREATE TABLE tagged (contact_id INTEGER, tag_id INTEGER)")
        conn.execute("CREATE TABLE tags (id INTEGER PRIMARY KEY, slug TEXT)")
        conn.execute("INSERT INTO contacts (subscribed) VALUES (1), (1), (0)")
        conn.execute("INSERT INTO tags (id, slug) VALUES (1, 'test-tag')")
        conn.execute("INSERT INTO tagged (contact_id, tag_id) VALUES (1, 1), (2, 1)")

        broadcast = Broadcast(send_to_tag="test-tag")
        count = broadcast.contact_count(conn)
        self.assertEqual(count, 2)

        broadcast = Broadcast(send_to_tag="*")
        count = broadcast.contact_count(conn)
        self.assertEqual(count, 2)

if __name__ == '__main__':
    unittest.main()
