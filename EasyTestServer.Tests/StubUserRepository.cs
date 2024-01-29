namespace EasyTestServer.Tests;

public class StubUserRepository : IUserRepository
{
    public Task CreateAsync(User entity)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetAsync(Guid id)
    {
        return await Task.FromResult(new User("jean michel stub"));
    }

    public void Update(User entity)
    {
        throw new NotImplementedException();
    }
}