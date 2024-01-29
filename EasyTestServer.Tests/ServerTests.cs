using NSubstitute;

namespace EasyTestServer.Tests;

public class ServerTests
{
    [Fact]
    public async Task Should_ReturnExpectedUserName()
    {
        //arrange
        var testServer = new Server()
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        // setup user to get
        var createResponse = await httpClient.PostAsJsonAsync("api/users", new CreateUserRequest("jean michel"));
        var id = (await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>())!.Id;

        //act
        var response = await httpClient.GetAsync($"api/users/{id}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetUserResponse>();
        content!.Name.Should().Be("jean michel");
    }

    [Fact]
    public async Task Should_ReturnUserNameFromStub_When_WithServiceReplaceServiceByStub()
    {
        //arrange
        var testServer = new Server()
            .WithService<IUserRepository>(new StubUserRepository())
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/users/{Guid.NewGuid()}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetUserResponse>();
        content!.Name.Should().Be("jean michel stub");
    }
    
    [Fact]
    public async Task Should_ReturnUserNameFromSubstitute_When_WithSubstituteReplaceServiceBySubstitute()
    {
        //arrange
        var testServer = new Server()
            .WithSubstitute<IUserRepository>(out var substitute)
            .Build<Program>();
        
        substitute.GetAsync(Arg.Any<Guid>()).ReturnsForAnyArgs(new User("jean michel substitute"));
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/users/{Guid.NewGuid()}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetUserResponse>();
        content!.Name.Should().Be("jean michel substitute");
    }
    
    [Fact]
    public async Task Should_ReturnExpectedSettingValue_When_WithSettingIsUsed()
    {
        //arrange
        const string settingKey = "TestSetting";
        
        var testServer = new Server()
            .WithSetting(key: settingKey, value: "Expected")
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/settings/{settingKey}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetSettingResponse>();
        content!.Value.Should().Be("Expected");
    }
    
    [Fact]
    public async Task Should_ReturnAppSettingsValue_When_WithSettingIsNotUsed()
    {
        //arrange
        const string settingKey = "TestSetting";
        
        var testServer = new Server()
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/settings/{settingKey}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetSettingResponse>();
        content!.Value.Should().Be("FromAppSettings");
    }
    
    [Fact]
    public async Task Should_ReturnError500_When_WithoutServiceRemoveRequiredService()
    {
        //arrange
        var testServer = new Server()
            .WithoutService<IUserRepository>()
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/users/{Guid.NewGuid()}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}