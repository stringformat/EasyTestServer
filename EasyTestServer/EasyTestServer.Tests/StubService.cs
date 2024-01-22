using EasyTestServer.Tests.Api;
using EasyTestServer.Tests.Api.Domain;

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