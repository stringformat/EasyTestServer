namespace EasyTestServer.Tests;

public class ServerDatabaseTests
{
    [Fact]
    public async Task Should_ReturnNameFromUser1_When_UseInMemoryDatabaseIsUsedAndWithDataHasAddedUser1()
    {
        //arrange
        var user1 = new User("jean charles");
        var user2 = new User("jean paul");
        
        var testServer = new Server()
            .UseDatabase()
                .WithData(user1)
                .WithData(user2)
                .Build<UserContext>()
            .Build<Program>();
        
        var httpClient = testServer.CreateClient();

        //act
        var response = await httpClient.GetAsync($"api/users/{user1.Id}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetUserResponse>();
        content!.Name.Should().Be("jean charles");
    }
}