using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EasyTestServer.Tests.Logger;

public class XUnitLoggerProvider(ITestOutputHelper testOutputHelper) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(testOutputHelper);
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}