using Npgsql;
using System.Dynamic;
using System.Data;
using System.Collections.Specialized;
using System.Text;
namespace Tailwind.Data;
public static class StringExtensions{


  public static string ToSnakeCase(this string text)
  {
      if(text == null) {
          throw new ArgumentNullException(nameof(text));
      }
      if(text.Length < 2) {
          return text;
      }
      if(text == "ID") {
          return "id";
      }
      var sb = new StringBuilder();
      sb.Append(char.ToLowerInvariant(text[0]));
      for(int i = 1; i < text.Length; ++i) {
          char c = text[i];
          if(char.IsUpper(c)) {
              sb.Append('_');
              sb.Append(char.ToLowerInvariant(c));
          } else {
              sb.Append(c);
          }
      }
      return sb.ToString();
  }
}
public static class ObjectExtensions{
  public static string ToValueList(this object o)
  {
    var expando = o.ToExpando();
    var values = (IDictionary<string, object>)expando;
    return string.Join(", ", values.Keys.Select(k => $"@{k}"));
  }
  public static string ToSettingList(this object o)
  {
    var expando = o.ToExpando();
    var values = (IDictionary<string, object>)expando;
    return string.Join(", ", values.Keys.Select(k => $"{k}=@{k}"));
  }
  public static string ToColumnList(this object o)
  {
    var expando = o.ToExpando();
    var values = (IDictionary<string, object>)expando;
    return string.Join(", ", values.Keys);
  }
  public static dynamic ToExpando(this object o)
    {
      if (o.GetType() == typeof(ExpandoObject)) return o; //shouldn't have to... but just in case
      var result = new ExpandoObject();
      var d = result as IDictionary<string, object>; //work with the Expando as a Dictionary
      if (o.GetType() == typeof(NameValueCollection) || o.GetType().IsSubclassOf(typeof(NameValueCollection)))
      {
        var nv = (NameValueCollection)o;
        nv.Cast<string>().Select(key => new KeyValuePair<string, object>(key, nv[key])).ToList().ForEach(i => d.Add(i));
      }
      else
      {
        var props = o.GetType().GetProperties();
        foreach (var item in props)
        {
          d.Add(item.Name, item.GetValue(o, null));
        }
      }
      return result;
    }

  /// <summary>
  /// Turns the object into a Dictionary
  /// </summary>
  public static IDictionary<string, object> ToDictionary(this object thingy)
  {
    return (IDictionary<string, object>)thingy.ToExpando();
  }

}

public static class CommandExtensions
{


  public static NpgsqlCommand AddParams(this NpgsqlCommand cmd, object o){
    var expando = o.ToExpando();
    var values = (IDictionary<string, object>)expando;
    return AddParamsDictionary(cmd, values);
  }
  public static NpgsqlCommand AddParamsDictionary(this NpgsqlCommand cmd, IDictionary<string, object> values){
    if (values != null)
    {
      foreach (var item in values)
      {
        if(item.Value != null){
          cmd.Parameters.AddWithValue(item.Key, item.Value);
          //Console.WriteLine($"{item.Key} = {item.Value}");
        }
        
      }
    }
    return cmd;
  }


  public static NpgsqlCommand Where(this NpgsqlCommand cmd, object o)
  {
    var expando = o.ToExpando();
    var parameters = (IDictionary<string, object>)expando;
    //var keys = parameters.Keys.Where(k => k != null).Select(k => $"{k}=@{k}");
    cmd.AddParams(o);
    var keys = cmd.Parameters.Select(p => $"{p.ParameterName}=@{p.ParameterName}");
    cmd.CommandText += $" where {string.Join(" and ", keys)}";
    
    return cmd;
  }
  public static NpgsqlCommand Limit(this NpgsqlCommand cmd, int limit)
  {
    cmd.CommandText += $" limit {limit}";
    return cmd;
  }

  public static NpgsqlCommand Order(this NpgsqlCommand cmd, string column = "id", string direction = "asc")
  {
    cmd.CommandText += $" order by {column} {direction}";
    return cmd;
  }

  public static List<dynamic> ToExpandoList(this IDataReader rdr)
  {
    var result = new List<dynamic>();
    while (rdr.Read())
    {
      result.Add(rdr.RecordToExpando());
    }
    return result;
  }
  public static dynamic RecordToExpando(this IDataReader rdr)
  {
    dynamic e = new ExpandoObject();
    var d = e as IDictionary<string, object>;
    object[] values = new object[rdr.FieldCount];
    rdr.GetValues(values);
    for (int i = 0; i < values.Length; i++)
    {
      var v = values[i];
      d.Add(rdr.GetName(i), DBNull.Value.Equals(v) ? null : v);
    }
    return e;
  }
 
}