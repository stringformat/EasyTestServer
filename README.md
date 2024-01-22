EasyTestServer
========

[![nuget](https://img.shields.io/badge/nuget-EasyTestServer-blue)](https://www.nuget.org/packages/EasyTestServer)
[![nuget](https://img.shields.io/badge/nuget-EasyTestServer.EntityFramework-blue)](https://www.nuget.org/packages/EasyTestServer.EntityFramework)

### What is it?

A tool for smooth, easy use of Microsoft's web host and in-memory test server

### Installation

* [EasyTestServer package](https://www.nuget.org/packages/EasyTestServer)
* [EasyTestServer.EntityFramework package](https://www.nuget.org/packages/EasyTestServer.EntityFramework)

### Basic use

Suppose we have a basic user management API, it can:
- Add a user
- Retrieve user information

This API uses a database with EntityFramework and very simple services.

```csharp
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
```

We simply want to test user creation route, but it's complicated to initialize data in a local database, to substitute services...

You can simply use EasyTestServer to initialize your test with the program you want.

```csharp
[Fact]
public async Task Should_ReturnExpectedValue()
{
    // Arrange
    var testServer = new EasyTestServer()
        .Build<Program>();
    
    var httpClient = testServer.CreateClient();

    // create user to retrieve
    var createRequest = new CreateUserRequest("jean michel");
    var createResponse = await httpClient.PostAsJsonAsync("api/users", createRequest);
    var id = (await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>())!.Id;

    // Act
    var getResponse = await httpClient.GetAsync($"api/users/{id}");
    
    // Assert
    await TestHelper.AssertResponse(getResponse, HttpStatusCode.OK, "jean michel");
}
```

In another case, you want to replace a service in dependency injection with a stub.

```csharp
public interface IUserService
{
    Task<Guid> CreateAsync(string name);
    Task<User?> GetAsync(Guid id);
}

public class StubService : IUserService
{
    ...

    public async Task<User?> GetAsync(Guid id)
    {
        return await Task.FromResult(new User("jean michel stub"));
    }
}
```

You can simply use '.WithService' to replace a service with your stub :

```csharp
[Fact]
public async Task Should_ReturnValueFromStub_When_ReplaceServiceByStub()
{
    //arrange
    var testServer = new EasyTestServer()
        .WithService<IUserService>(new StubService())
        .Build<Program>();
    
    var httpClient = testServer.CreateClient();

    //act
    var response = await httpClient.GetAsync($"api/users/{Guid.NewGuid()}");
    
    //assert
    await TestHelper.AssertResponse(response, HttpStatusCode.OK, "jean michel stub");
}
```

And you can do the same to use NSubstitute directly (Thank's to https://github.com/nsubstitute/NSubstitute) :

```csharp
[Fact]
public async Task Should_ReturnValueFromSubstitute_ReplaceServiceBySubstitute()
{
    //arrange
    var testServer = new EasyTestServer()
        .WithSubstitute<IUserService>(out var substitute)
        .Build<Program>();
    
    substitute.GetAsync(Arg.Any<Guid>()).ReturnsForAnyArgs(new User("jean michel substitute"));
    
    var httpClient = testServer.CreateClient();

    //act
    var response = await httpClient.GetAsync($"api/users/{Guid.NewGuid()}");
    
    //assert
    await TestHelper.AssertResponse(response, HttpStatusCode.OK, "jean michel substitute");
}
```

### Advanced use

You can go even further by replacing the local database with an in-memory one.
To do this, use the "EasyTestServer.EntityFramework" package.

You can use 'UseDatabase' to replace your database with an in-memory one.
You can initialize data for your test with 'WithData'.

```csharp
[Fact]
public async Task Should_ReturnNameFromUser1_When_UseInMemoryDatabaseIsUsedWithUser1Initialized()
{
    //arrange
    var user1 = new User("jean charles");
    var user2 = new User("jean paul");
    
    var testServer = new EasyTestServer()
        .UseDatabase()
            .WithData(user1)
            .WithData(user2)
            .Build<UserContext>()
        .Build<Program>();
    
    var httpClient = testServer.CreateClient();

    //act
    var response = await httpClient.GetAsync($"api/users/{user1.Id}");
    
    //assert
    await TestHelper.AssertResponse(response, HttpStatusCode.OK, "jean charles");
}
```

### Thank's to

- https://github.com/nsubstitute/NSubstitute
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
