#region License
// author:         Wu, Gary
// created:        2:58 PM
// description:
#endregion

using Serilog.Core;
using Serilog.Events;
using Xunit.Abstractions;

namespace CommonUtils.UnitTestHelpers;

public class TestOutputHelperSink : ILogEventSink
{
    private readonly ITestOutputHelper _output;

    public TestOutputHelperSink(ITestOutputHelper output)
    {
        _output = output;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        _output.WriteLine(message);
    }
}