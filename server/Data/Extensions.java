package server.Data;

import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.HashMap;
import java.util.Map;

public class Extensions {

    public static String toSnakeCase(String text) {
        if (text == null) {
            throw new IllegalArgumentException("text cannot be null");
        }
        if (text.length() < 2) {
            return text;
        }
        if (text.equals("ID")) {
            return "id";
        }
        StringBuilder sb = new StringBuilder();
        sb.append(Character.toLowerCase(text.charAt(0)));
        for (int i = 1; i < text.length(); ++i) {
            char c = text.charAt(i);
            if (Character.isUpperCase(c)) {
                sb.append('_');
                sb.append(Character.toLowerCase(c));
            } else {
                sb.append(c);
            }
        }
        return sb.toString();
    }

    public static Map<String, Object> toMap(Object o) {
        Map<String, Object> map = new HashMap<>();
        for (var field : o.getClass().getDeclaredFields()) {
            field.setAccessible(true);
            try {
                map.put(field.getName(), field.get(o));
            } catch (IllegalAccessException e) {
                throw new RuntimeException(e);
            }
        }
        return map;
    }

    public static String toValueList(Object o) {
        Map<String, Object> map = toMap(o);
        StringBuilder sb = new StringBuilder();
        for (String key : map.keySet()) {
            sb.append("@").append(key).append(", ");
        }
        return sb.substring(0, sb.length() - 2);
    }

    public static String toSettingList(Object o) {
        Map<String, Object> map = toMap(o);
        StringBuilder sb = new StringBuilder();
        for (String key : map.keySet()) {
            sb.append(key).append("=@").append(key).append(", ");
        }
        return sb.substring(0, sb.length() - 2);
    }

    public static String toColumnList(Object o) {
        Map<String, Object> map = toMap(o);
        StringBuilder sb = new StringBuilder();
        for (String key : map.keySet()) {
            sb.append(key).append(", ");
        }
        return sb.substring(0, sb.length() - 2);
    }

    public static Map<String, Object> recordToMap(ResultSet rs) throws SQLException {
        Map<String, Object> map = new HashMap<>();
        int columnCount = rs.getMetaData().getColumnCount();
        for (int i = 1; i <= columnCount; i++) {
            map.put(rs.getMetaData().getColumnName(i), rs.getObject(i));
        }
        return map;
    }
}
