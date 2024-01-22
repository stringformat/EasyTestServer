namespace EasyTestServer.Tests;

public class StubService : IUserService
{
    public Task<Guid> CreateAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetAsync(Guid id)
    {
        return await Task.FromResult(new User("jean michel stub"));
    }
}