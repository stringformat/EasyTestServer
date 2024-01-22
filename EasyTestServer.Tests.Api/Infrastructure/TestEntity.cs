using Microsoft.EntityFrameworkCore;

namespace EasyTestServer.Tests.Api.Infrastructure;

[PrimaryKey("Id")]
public class TestEntity
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Value { get; init; }
}