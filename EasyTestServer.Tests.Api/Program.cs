using EasyTestServer.Tests.Api.Api;
using EasyTestServer.Tests.Api.Domain;
using EasyTestServer.Tests.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddDbContext<UserContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase("TestDb"));

var app = builder.Build();

app.MapPost("api/users", async (CreateUserRequest request, IUserService service) =>
{
    var createdUserId = await service.CreateAsync(request.Name);
    
    return Results.CreatedAtRoute("GetUser", new { id = createdUserId },new CreateUserResponse(createdUserId));
});

app.MapGet("api/users/{id:guid}", async (Guid id, IUserService service) =>
{
    var user = await service.GetAsync(id);
    
    return user is null ? Results.NotFound() : Results.Ok(new GetUserResponse(user.Name));
}).WithName("GetUser");

app.Run();