using EasyTestServer.Tests.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EasyTestServer.Tests.Api.Infrastructure;

public interface IUserRepository
{
    Task CreateAsync(User entity);
    Task<User?> GetAsync(Guid id);
    void Update(User entity);
}

public class UserRepository(UserContext context) : IUserRepository
{
    public async Task CreateAsync(User entity)
    {
        await context.Set<User>().AddAsync(entity);
    }
    
    public void Update(User entity)
    {
        context.Set<User>().Update(entity);
    }

    public async Task<User?> GetAsync(Guid id)
    {
        return await context
            .Set<User>()
            .SingleOrDefaultAsync(x => x.Id == id);
    }
}