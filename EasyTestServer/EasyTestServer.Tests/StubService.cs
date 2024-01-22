using EasyTestServer.Tests.Api;

namespace EasyTestServer.Tests;

public class StubService : IService
{
    public Task<Guid> CreateValueAsync(string value)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> GetValueAsync(Guid id)
    {
        return await Task.FromResult("test value from stub");
    }
}