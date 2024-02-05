namespace EasyTestServer.Core;

public class ServerOptions
{
    public string? Environment { get; set; } = null;
    public string? ContentRoot { get; set; } = null;
    public Uri? BaseAddress { get; set; } = null;
    public bool DisableLogging { get; set; } = false;
    public bool DisableAuthentication { get; set; } = false;
    public string[] Urls { get; set; } = [];
}