using System.Net;
using EasyTestServer.EntityFramework;
using EasyTestServer.Tests.Api.Domain;
using EasyTestServer.Tests.Api.Infrastructure;

namespace EasyTestServer.Tests;

public class EasyTestServerDatabaseTests
{
    [Fact]
    public async Task Should_ReturnNameFromUser1_When_UseInMemoryDatabaseIsUsedWithUser1Initialized()
    {
        //arrange
        var user1 = new User("jean charles");
        var user2 = new User("jean paul");
        
        var testServer = new Core.EasyTestServer()
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
}