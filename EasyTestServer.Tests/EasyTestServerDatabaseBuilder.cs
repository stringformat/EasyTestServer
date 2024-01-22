using System.Net;
using EasyTestServer.Core;
using EasyTestServer.EntityFramework;
using EasyTestServer.Tests.Api.Infrastructure;

namespace EasyTestServer.Tests;

public class EasyTestServerDatabaseBuilder
{
    [Fact]
    public async Task Should_ReturnValueFromEntity_When_UseInMemoryDatabaseIsUsedWithTestEntity()
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
        await TestHelper.AssertResponse(response, HttpStatusCode.OK, "test value");
    }
}