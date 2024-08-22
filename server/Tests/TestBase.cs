using System.Data;
using Tailwind.Data;
using Tailwind.Mail.Models;

public abstract class TestBase : IDisposable
{
    public IDbConnection Conn { get; set; }
    protected TestBase()
    {
      Viper.Test();
      Conn = DB.Postgres();
    }

    public void Dispose()
    {
        // Do "global" teardown here; Called after every test method.
        Conn.Close();
        Conn.Dispose();
    }
}