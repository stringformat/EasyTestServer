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

        var createRequest = new CreateRequest("test value");
        var createResponse = await httpClient.PostAsJsonAsync("api/create-value", createRequest);
        var id = (await createResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        //act
        var getResponse = await httpClient.GetAsync($"api/get-value/{id}");
        
        //assert
        await TestHelper.AssertResponse(getResponse, HttpStatusCode.OK, "test value");
    }

    [Fact]
    public async Task Should_ReturnValueFromStub_When_ReplaceServiceByStub()
    {
        //arrange
        var testServer = new EasyTestServerBuilder()
            .WithService<IService>(new StubService())
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/get-value/{Guid.NewGuid()}");
        
        //assert
        await TestHelper.AssertResponse(response, HttpStatusCode.OK, "test value from stub");
    }
    
    [Fact]
    public async Task Should_ReturnValueFromSubstitute_ReplaceServiceBySubstitute()
    {
        //arrange
        var testServer = new EasyTestServerBuilder()
            .WithSubstitute<IService>(out var substitute)
            .Build<Program>();
        
        substitute.GetValueAsync(Arg.Any<Guid>()).ReturnsForAnyArgs("test value from substitute");
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/get-value/{Guid.NewGuid()}");
        
        //assert
        await TestHelper.AssertResponse(response, HttpStatusCode.OK, "test value from substitute");
    }
    
    [Fact]
    public async Task Should_ReturnError500_When_RemoveService()
    {
        //arrange
        var testServer = new EasyTestServerBuilder()
            .WithoutService<IService>()
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/get-value/{Guid.NewGuid()}");
        
        //assert
        await TestHelper.AssertResponse(response, HttpStatusCode.InternalServerError);
    }
}