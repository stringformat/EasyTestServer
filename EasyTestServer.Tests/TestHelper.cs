using System.Net;
using System.Net.Http.Json;
using EasyTestServer.Tests.Api.Api;
using FluentAssertions;

namespace EasyTestServer.Tests;

public static class TestHelper
{
    public static async Task AssertResponse(HttpResponseMessage response, HttpStatusCode expectedCode, string? expectedName = null)
    {
        response.StatusCode.Should().Be(expectedCode);

        if (expectedName is not null)
        {
            var content = await response.Content.ReadFromJsonAsync<GetUserResponse>();
            content!.Name.Should().Be(expectedName);
        }
    }
}