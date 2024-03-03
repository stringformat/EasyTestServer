using EasyTestServer.Tests.Api;
using EasyTestServer.Tests.Api.Api;
using EasyTestServer.Tests.Api.Domain;
using EasyTestServer.Tests.Api.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddDbContext<UserContext>(optionsBuilder => optionsBuilder.UseSqlite("Data Source=../EasyTestServer.Tests.Api/test.db"));
builder.Services.AddSingleton<TokenService>();

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey("e1d4152c-5255-459a-9905-7818a04cd6ac"u8.ToArray()),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("manager", policy => policy.RequireRole("manager"))
    .AddPolicy("operator", policy => policy.RequireRole("operator"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("api/users", async (CreateUserRequest request, IUserRepository repository, UserContext context) =>
{
    var user = new User(request.Name);
    
    await repository.CreateAsync(user);
    await context.SaveChangesAsync();
    
    return Results.CreatedAtRoute("GetUser", new { id = user.Id },new CreateUserResponse(user.Id));
});

app.MapGet("api/users/{id:guid}", async (Guid id, IUserRepository repository, ILogger<Program> logger) =>
{
    var user = await repository.GetAsync(id);
    
    logger.LogInformation($"Try to retrieve user '{id}'");
    
    return user is null ? Results.NotFound() : Results.Ok(new GetUserResponse(user.Name));
}).WithName("GetUser");

app.MapGet("api/login/{id:guid}", async (Guid id, IUserRepository repository, TokenService tokenService) =>
{
    var user = await repository.GetAsync(id);

    if (user is null)
        return Results.NotFound(new { message = "Invalid user" });

    var token = tokenService.GenerateToken(user, "operator");
    
    return Results.Ok(new LoginResponse(token));
});

app.MapGet("api/secure", (ILogger<Program> logger) =>
{
    logger.LogInformation("Access to secure route");

    return Task.FromResult(Results.Ok());
}).RequireAuthorization("operator");

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