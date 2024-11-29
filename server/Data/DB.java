package server.Data;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.util.Properties;

public class DB implements IDb {
    private static final String DATABASE_URL = System.getenv("DATABASE_URL");

    @Override
    public Connection connect() throws SQLException {
        return postgres();
    }

    public static Connection postgres() throws SQLException {
        if (DATABASE_URL == null || DATABASE_URL.isEmpty()) {
            throw new IllegalArgumentException("No DATABASE_URL found in environment");
        }

        Properties props = new Properties();
        props.setProperty("user", "yourusername");
        props.setProperty("password", "yourpassword");
        props.setProperty("ssl", "true");

        Connection conn = DriverManager.getConnection(DATABASE_URL, props);
        return conn;
    }
}
