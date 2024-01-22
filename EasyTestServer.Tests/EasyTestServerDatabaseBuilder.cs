using System.Net;
using EasyTestServer.Core;
using EasyTestServer.EntityFramework;
using EasyTestServer.Tests.Api.Domain;
using EasyTestServer.Tests.Api.Infrastructure;

namespace EasyTestServer.Tests;

public class EasyTestServerDatabaseBuilder
{
    [Fact]
    public async Task Should_ReturnValueFromEntity_When_UseInMemoryDatabaseIsUsedWithTestEntity()
    {
        //arrange
        var user = new User("jean charles");
        
        var testServer = new EasyTestServerBuilder()
            .UseDatabase().WithData(user).Build<UserContext>()
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/users/{user.Id}");
        
        //assert
        await TestHelper.AssertResponse(response, HttpStatusCode.OK, "jean charles");
    }
}