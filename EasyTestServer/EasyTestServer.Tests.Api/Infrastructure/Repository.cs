using Microsoft.EntityFrameworkCore;

namespace EasyTestServer.Tests.Api.Infrastructure;

public interface IRepository
{
    Task CreateAsync(TestEntity entity);
    Task<TestEntity?> GetAsync(Guid id);
}

public class Repository(TestContext context) : IRepository
{
    public async Task CreateAsync(TestEntity entity)
    {
        await context.Set<TestEntity>().AddAsync(entity);

        await context.SaveChangesAsync();
    }

    public async Task<TestEntity?> GetAsync(Guid id)
    {
        return await context
            .Set<TestEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);
    }
}