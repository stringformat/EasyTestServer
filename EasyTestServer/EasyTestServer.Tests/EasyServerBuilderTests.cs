using System.Net;
using System.Net.Http.Json;
using EasyTestServer.Builder;
using EasyTestServer.EntityFramework;
using EasyTestServer.Tests.Api;
using FluentAssertions;
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

        var createRequest = new Request("test value");
        var createResponse = await httpClient.PostAsJsonAsync("api/create-value", createRequest);
        var id = (await createResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        //act
        var getResponse = await httpClient.GetAsync($"api/get-value/{id}");
        
        //assert
        await AssertResponse(getResponse, HttpStatusCode.OK, "test value");
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
        await AssertResponse(response, HttpStatusCode.OK, "test value from stub");
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
        await AssertResponse(response, HttpStatusCode.OK, "test value from substitute");
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
        await AssertResponse(response, HttpStatusCode.InternalServerError);
    }
    
    [Fact]
    public async Task Test()
    {
        //arrange
        var testEntity = new TestEntity { Value = "test value"};
        
        var testServer = new EasyTestServerBuilder()
            .UseDatabase().WithData(testEntity).Build<TestContext>()
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/get-value/{testEntity.Id}");
        
        //assert
        await AssertResponse(response, HttpStatusCode.OK, "test value");
    }
    
    private static async Task AssertResponse(HttpResponseMessage response, HttpStatusCode expectedCode, string? expectedValue = null)
    {
        response.StatusCode.Should().Be(expectedCode);

        if (expectedValue is not null)
        {
            var content = await response.Content.ReadFromJsonAsync<GetResponse>();
            content!.Value.Should().Be(expectedValue);
        }
    }
}