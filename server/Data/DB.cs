using System.Data;
using Npgsql;
using Dapper;
using System.Reflection;
namespace Tailwind.Data
{
  public class CustomResolver : SimpleCRUD.IColumnNameResolver
  {
    public string ResolveColumnName(PropertyInfo propertyInfo)
    {
      return propertyInfo.Name.ToSnakeCase();
    }
  }

  public interface IDb{
    IDbConnection Connect();
  }

  public class DB: IDb
  {
    public IDbConnection Connect()
    {
      return DB.Postgres();
    }
    public static IDbConnection Postgres()
    {
      var connectionString = Viper.Config().Get("DATABASE_URL");
      
      if(String.IsNullOrEmpty(connectionString)){
        throw new InvalidProgramException("No DATABASE_URL found in environment");
      }

      Dapper.SimpleCRUD.SetDialect(Dapper.SimpleCRUD.Dialect.PostgreSQL);
      var resolver = new CustomResolver();
      SimpleCRUD.SetColumnNameResolver(resolver);
      var conn = new NpgsqlConnection(connectionString);
      conn.Open();
      
      return conn;
    }
  }
}