using EasyTestServer.Tests.Api.Infrastructure;

namespace EasyTestServer.Tests.Api.Domain;

public interface IUserService
{
    Task<Guid> CreateAsync(string name);
    Task<User?> GetAsync(Guid id);
}

public class UserService(IUserRepository repository) : IUserService
{
    public async Task<Guid> CreateAsync(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var user = new User(name);
        await repository.CreateAsync(user);

        return user.Id;
    }

    public async Task<User?> GetAsync(Guid id) => await repository.GetAsync(id);
}