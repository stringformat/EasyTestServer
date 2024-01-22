using EasyTestServer.Tests.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EasyTestServer.Tests.Api.Infrastructure;

public interface IUserRepository
{
    Task CreateAsync(User entity);
    Task<User?> GetAsync(Guid id);
}

public class UserRepository(UserContext context) : IUserRepository
{
    public async Task CreateAsync(User entity)
    {
        await context.Set<User>().AddAsync(entity);

        await context.SaveChangesAsync();
    }

    public async Task<User?> GetAsync(Guid id)
    {
        return await context
            .Set<User>()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);
    }
}