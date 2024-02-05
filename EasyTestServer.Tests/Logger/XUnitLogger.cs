using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EasyTestServer.Tests.Logger;

public class XUnitLogger(ITestOutputHelper testOutputHelper) : ILogger, IDisposable
{
    public void Dispose()
    {
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) 
    {
        testOutputHelper.WriteLine(state.ToString());
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) => this;
}