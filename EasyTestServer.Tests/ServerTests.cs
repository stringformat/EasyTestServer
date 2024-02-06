using EasyTestServer.Tests.Logger;
using NSubstitute;
using Xunit.Abstractions;

namespace EasyTestServer.Tests;

public class ServerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ServerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Should_ReturnExpectedUserName()
    {
        //arrange
        var httpClient = new Server()
            .Build<Program>();
        
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
        var httpClient = new Server()
            .WithService<IUserRepository>(new StubUserRepository())
            .Build<Program>();
        
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
        var httpClient = new Server()
            .WithSubstitute<IUserRepository>(out var substitute)
            .Build<Program>();
        
        substitute.GetAsync(Arg.Any<Guid>()).ReturnsForAnyArgs(new User("jean michel substitute"));
        
        //act
        var response = await httpClient.GetAsync($"api/users/{Guid.NewGuid()}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetUserResponse>();
        content!.Name.Should().Be("jean michel substitute");
    }
    
    [Fact]
    public async Task Should_Return401_When_AuthenticationIsEnable()
    {
        //arrange
        var httpClient = new Server()
            .Build<Program>();

        //act
        var responseMessage = await httpClient.GetAsync("api/secure");

        responseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Should_Return200_When_AuthenticationIsDisable()
    {
        //arrange
        var httpClient = new Server()
            .Build<Program>(options => options.DisableAuthentication = true);

        //act
        var responseMessage = await httpClient.GetAsync("api/secure");

        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Should_ReturnExpectedSettingValue_When_WithSettingIsUsed()
    {
        //arrange
        const string settingKey = "TestSetting";
        
        var httpClient = new Server()
            .WithSetting(key: settingKey, value: "Expected")
            .Build<Program>();

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
        
        var httpClient = new Server()
            .Build<Program>();

        //act
        var response = await httpClient.GetAsync($"api/settings/{settingKey}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetSettingResponse>();
        content!.Value.Should().Be("FromAppSettings");
    }
}