using EasyTestServer.Tests.Api.Api;
using EasyTestServer.Tests.Api.Domain;
using EasyTestServer.Tests.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IService, Service>();
builder.Services.AddTransient<IRepository, Repository>();
builder.Services.AddDbContext<TestContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase("TestDb"));

var app = builder.Build();

app.MapGet("api/get-value/{id:guid}", async (Guid id, IService service) =>
{
    var result = await service.GetValueAsync(id);

    return new GetResponse(result);
});

app.MapPost("api/create-value", async (CreateRequest request, IService service) =>
{
    var result = await service.CreateValueAsync(request.Value);
    
    return new CreateResponse(result);
});

app.Run();