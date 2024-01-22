using System.Net;
using System.Net.Http.Json;
using EasyTestServer.Core;
using EasyTestServer.Tests.Api.Api;
using EasyTestServer.Tests.Api.Domain;
using NSubstitute;

namespace EasyTestServer.Tests;

public class EasyServerBuilderTests
{
    [Fact]
    public async Task Should_ReturnExpectedValue()
    {
        //arrange
        var testServer = new EasyTestServerBuilder()
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        var createRequest = new CreateUserRequest("jean michel");
        var createResponse = await httpClient.PostAsJsonAsync("api/users", createRequest);
        var id = (await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>())!.Id;

        //act
        var getResponse = await httpClient.GetAsync($"api/users/{id}");
        
        //assert
        await TestHelper.AssertResponse(getResponse, HttpStatusCode.OK, "jean michel");
    }

    [Fact]
    public async Task Should_ReturnValueFromStub_When_ReplaceServiceByStub()
    {
        //arrange
        var testServer = new EasyTestServerBuilder()
            .WithService<IUserService>(new StubService())
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/users/{Guid.NewGuid()}");
        
        //assert
        await TestHelper.AssertResponse(response, HttpStatusCode.OK, "jean michel stub");
    }
    
    [Fact]
    public async Task Should_ReturnValueFromSubstitute_ReplaceServiceBySubstitute()
    {
        //arrange
        var testServer = new EasyTestServerBuilder()
            .WithSubstitute<IUserService>(out var substitute)
            .Build<Program>();
        
        substitute.GetAsync(Arg.Any<Guid>()).ReturnsForAnyArgs(new User("jean michel substitute"));
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/users/{Guid.NewGuid()}");
        
        //assert
        await TestHelper.AssertResponse(response, HttpStatusCode.OK, "jean michel substitute");
    }
    
    [Fact]
    public async Task Should_ReturnError500_When_RemoveService()
    {
        //arrange
        var testServer = new EasyTestServerBuilder()
            .WithoutService<IUserService>()
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/get-value/{Guid.NewGuid()}");
        
        //assert
        await TestHelper.AssertResponse(response, HttpStatusCode.InternalServerError);
    }
}