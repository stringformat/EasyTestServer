namespace EasyTestServer.Tests.Api;

public interface IService
{
    Task<Guid> CreateValueAsync(string value);
    Task<string?> GetValueAsync(Guid id);
}

public class Service(IRepository repository) : IService
{
    public async Task<Guid> CreateValueAsync(string value)
    {
        var testEntity = new TestEntity
        {
            Value = value
        };
        await repository.CreateAsync(testEntity);

        return testEntity.Id;
    }

    public async Task<string?> GetValueAsync(Guid id) => (await repository.GetAsync(id))?.Value;
}