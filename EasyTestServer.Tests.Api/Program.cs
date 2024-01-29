using EasyTestServer.Tests.Api.Api;
using EasyTestServer.Tests.Api.Domain;
using EasyTestServer.Tests.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddDbContext<UserContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase("TestDb"));

var app = builder.Build();

app.MapPost("api/users", async (CreateUserRequest request, IUserRepository repository, UserContext context) =>
{
    var user = new User(request.Name);
    
    await repository.CreateAsync(user);
    await context.SaveChangesAsync();
    
    return Results.CreatedAtRoute("GetUser", new { id = user.Id },new CreateUserResponse(user.Id));
});

app.MapGet("api/users/{id:guid}", async (Guid id, IUserRepository repository) =>
{
    var user = await repository.GetAsync(id);
    
    return user is null ? Results.NotFound() : Results.Ok(new GetUserResponse(user.Name));
}).WithName("GetUser");

app.MapPost("api/users/{id:guid}/friends", async (Guid id, 
    AddFriendRequest request, 
    UserContext context, 
    IUserRepository repository) =>
{
    var user = await repository.GetAsync(id);

    if (user is null)
        return Results.NotFound();

    var friend = new Friend(request.Name);
    user.AddFriend(friend);
    
    repository.Update(user);
    await context.SaveChangesAsync();
    
    return Results.CreatedAtRoute("GetFriend", new { userId = user.Id, friendId = friend.Id },new AddFriendResponse(friend.Id));
});

app.MapGet("api/users/{userId:guid}/friends/{friendId:guid}", async (Guid userId, Guid friendId, IUserRepository repository) =>
{
    var user = await repository.GetAsync(userId);
    
    if (user is null)
        return Results.NotFound();
    
    var friend = user.Friends.SingleOrDefault(x => x.Id == friendId);
    
    return friend is null ? Results.NotFound() : Results.Ok(new GetFriendResponse(friend.Name));
}).WithName("GetFriend");

app.MapGet("api/settings/{key}", (string key, IConfiguration configuration) =>
{
    var setting = configuration[key];

    return setting is null ? Results.NotFound() : Results.Ok(new GetSettingResponse(setting));
});

app.Run();