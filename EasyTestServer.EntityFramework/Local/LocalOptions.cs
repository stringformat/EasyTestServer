namespace EasyTestServer.EntityFramework.Local;

public class LocalOptions
{
    public LocalDbType DbType { get; set; } = LocalDbType.SqlServer;
    public string ConnectionString { get; set; } = string.Empty;
    public bool UseTemporaryDatabase { get; set; } = true;
    public bool PreserveTemporaryDatabase { get; set; } = false;
}