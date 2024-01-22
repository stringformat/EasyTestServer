using System.Net;
using System.Net.Http.Json;
using EasyTestServer.Tests.Api.Api;
using FluentAssertions;

namespace EasyTestServer.Tests;

public static class TestHelper
{
    public static async Task AssertResponse(HttpResponseMessage response, HttpStatusCode expectedCode, string? expectedValue = null)
    {
        response.StatusCode.Should().Be(expectedCode);

        if (expectedValue is not null)
        {
            var content = await response.Content.ReadFromJsonAsync<GetResponse>();
            content!.Value.Should().Be(expectedValue);
        }
    }
}