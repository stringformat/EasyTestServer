namespace EasyTestServer.Tests.Api.Domain;

public class Friend
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }

    public Friend(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        Name = name;
    }

    //ORM
    private Friend()
    {
    }
}