using ProcessDoctor.Backend.Linux.Tests.ProcTests;
using ProcessDoctor.TestFramework.Logging;
using Xunit.Abstractions;

namespace ProcessDoctor.Backend.Linux.Tests;

public sealed class ProcessListSnapshotTests(ITestOutputHelper output) : ProcTestsBase
{
    [Fact]
    public void Should_enumerate_processes_properly()
    {
        using var logger = new ThrowingLoggerAdapter(nameof(ProcessProviderTests), output);
    }

    [Fact]
    public void Should_ignore_directories_which_do_not_belong_to_processes()
    {
        using var logger = new ThrowingLoggerAdapter(nameof(ProcessProviderTests), output);
    }

    [Fact]
    public void Should_return_empty_enumeration_if_error_occurrs()
    {
        using var logger = new ThrowingLoggerAdapter(nameof(ProcessProviderTests), output);
    }
}
