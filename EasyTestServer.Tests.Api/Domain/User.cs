namespace EasyTestServer.Tests.Api.Domain;

public class User
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    
    public User(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        
        Name = name;
    }

    //ORM
    private User()
    {
    }
}