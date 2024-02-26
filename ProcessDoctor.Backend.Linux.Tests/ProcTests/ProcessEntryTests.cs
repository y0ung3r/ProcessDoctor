using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using ProcessDoctor.Backend.Linux.Proc;

namespace ProcessDoctor.Backend.Linux.Tests.ProcTests;

public sealed class ProcessEntryTests : ProcTestsBase
{
    [Fact]
    public void Should_throw_exception_if_directory_is_not_process()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var directoryInfo = fileSystem.DirectoryInfo.New("unknown");

        // Act
        this.Invoking(_ => ProcessEntry.Create(directoryInfo))
            .Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Should_read_process_command_line_properly()
    {
        // Arrange
        const string expectedCommandLine = "test command line";

        var processDirectory = CreateTestProcess(id: 1);
        CreateCommandLineFile(id: 1, expectedCommandLine);

        var sut = ProcessEntry.Create(processDirectory);

        // Act & Assert
        sut.CommandLine
            .Should()
            .Be(expectedCommandLine);
    }

    [Fact]
    public void Command_line_should_be_null_if_value_is_empty()
    {
        // Arrange
        var processDirectory = CreateTestProcess(id: 1);
        CreateCommandLineFile(id: 1, string.Empty);

        var sut = ProcessEntry.Create(processDirectory);

        // Act & Assert
        sut.CommandLine
            .Should()
            .BeNull();
    }

    [Fact]
    public void Should_read_process_status_section_properly()
    {
        // Arrange
        const string expectedStatusContent =
            """
                Name: ProcessDoctor
                ...
                ...
                ...
                Pid: 1
            """;

        var processDirectory = CreateTestProcess(id: 1);
        CreateStatusFile(id: 1, expectedStatusContent);

        var sut = ProcessEntry.Create(processDirectory);

        // Act
        sut.Invoking(processEntry => processEntry.Status)
            .Should()
            .NotThrow();

        sut.Status
            .Should()
            .NotBeNull();
    }
}
