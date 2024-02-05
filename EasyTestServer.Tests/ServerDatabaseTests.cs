namespace EasyTestServer.Tests;

public class ServerDatabaseTests
{
    [Fact]
    public async Task Should_ReturnNameFromUser1_When_UseInMemoryDatabaseIsUsedAndWithDataHasAddedUser1()
    {
        //arrange
        var user1 = new User("jean charles");
        var user2 = new User("jean paul");
        
        var httpClient = new Server()
            .UseDatabase()
                .WithData(user1)
                .WithData(user2)
                .Build<UserContext>()
            .Build<Program>();

        //act
        var response = await httpClient.GetAsync($"api/users/{user1.Id}");
        
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<GetUserResponse>();
        content!.Name.Should().Be("jean charles");
    }
    
    [Fact]
    public async Task Should_ReturnFriendFromUser_When_WithDataHasAddedUserWithFriend()
    {
        //arrange
        var user = new User("jean charles");
        var friend = new Friend("jean marie");
        user.Friends.Add(friend);
        
        var httpClient = new Server()
            .UseDatabase()
                .WithData(user)
                .Build<UserContext>(useInMemoryDb: false)
            .Build<Program>();
        
        //act
        var response = await httpClient.GetFromJsonAsync<GetFriendResponse>($"api/users/{user.Id}/friends/{friend.Id}");

        //assert
        response?.Name.Should().Be("jean marie");
    }
}