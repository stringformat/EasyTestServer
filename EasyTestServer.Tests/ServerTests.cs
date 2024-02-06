using System.Net.Http.Headers;
using NSubstitute;

namespace EasyTestServer.Tests;

public class ServerTests
{
    [Fact]
    public async Task Should_ReturnExpectedUserName()
    {
        //arrange
        var httpClient = new Server<Program>()
            .Build();
        
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
        var httpClient = new Server<Program>()
            .WithService<IUserRepository>(new StubUserRepository())
            .Build();
        
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
        var httpClient = new Server<Program>()
            .WithSubstitute<IUserRepository>(out var substitute)
            .Build();
        
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
        var httpClient = new Server<Program>()
            .Build();

        //act
        var responseMessage = await httpClient.GetAsync("api/secure");

        responseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Should_Return200_When_AuthenticationIsDisable()
    {
        //arrange
        var httpClient = new Server<Program>()
            .Build();
        
        var createResponse = await httpClient.PostAsJsonAsync("api/users", new CreateUserRequest("jean michel"));
        var id = (await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>())!.Id;

        //act
        //var login = await httpClient.GetAsync($"api/login/{id}");
        //var content = await login.Content.ReadFromJsonAsync<LoginResponse>();


        //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.Token);
        var test = await httpClient.GetAsync("api/secure");

        //responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Should_ReturnExpectedSettingValue_When_WithSettingIsUsed()
    {
        //arrange
        const string settingKey = "TestSetting";
        
        var httpClient = new Server<Program>()
            .WithSetting(key: settingKey, value: "Expected")
            .Build();

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
        
        var httpClient = new Server<Program>()
            .Build();

        //act
        var response = await httpClient.GetAsync($"api/settings/{settingKey}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetSettingResponse>();
        content!.Value.Should().Be("FromAppSettings");
    }
}