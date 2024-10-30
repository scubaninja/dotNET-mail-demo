package server.Tests;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.AfterEach;
import server.Data.DB;

import java.sql.Connection;
import java.sql.SQLException;

public abstract class TestBase {

    protected Connection conn;

    @BeforeEach
    public void setUp() throws SQLException {
        conn = new DB().connect();
    }

    @AfterEach
    public void tearDown() throws SQLException {
        if (conn != null && !conn.isClosed()) {
            conn.close();
        }
    }
}
