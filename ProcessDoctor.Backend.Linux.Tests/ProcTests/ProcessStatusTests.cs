using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using ProcessDoctor.Backend.Linux.Proc;
using ProcessDoctor.Backend.Linux.Proc.Enums;

namespace ProcessDoctor.Backend.Linux.Tests.ProcTests;

public sealed class ProcessStatusTests : ProcTestsBase
{
    [Fact]
    public void Should_throw_exception_if_file_has_different_name()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var fileInfo = fileSystem.FileInfo.New("unknown");

        // Act
        this.Invoking(_ => ProcessStatus.Create(fileInfo))
            .Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void Should_read_process_name_properly()
    {
        // Arrange
        const string expectedProcessName = "ProcessDoctor";
        const string statusContent =
            $"""
                Name: {expectedProcessName}
                ...
                ...
                ...
                Pid: 1
            """;

        var statusFile = CreateStatusFile(id: 1, statusContent);
        var sut = ProcessStatus.Create(statusFile);

        // Act & Assert
        sut.Name
            .Should()
            .Be(expectedProcessName);
    }

    [Theory]
    [InlineData("R (running)", ProcessState.Running)]
    [InlineData("S (sleeping)", ProcessState.Sleeping)]
    [InlineData("D", ProcessState.UninterruptibleWait)]
    [InlineData("Z", ProcessState.Zombie)]
    [InlineData("T", ProcessState.TracedOrStopped)]
    public void Should_read_process_state_properly(string rawState, ProcessState expectedState)
    {
        // Arrange
        var statusContent =
            $"""
                 Name: ProcessDoctor
                 ...
                 State: {rawState}
                 ...
                 Pid: 1
             """;

        var statusFile = CreateStatusFile(id: 1, statusContent);
        var sut = ProcessStatus.Create(statusFile);

        // Act & Assert
        sut.State
            .Should()
            .Be(expectedState);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("P")]
    [InlineData("X")]
    [InlineData("A")]
    public void Should_throw_exception_if_process_state_has_invalid_value(string rawState)
    {
        // Arrange
        var statusContent =
            $"""
                 Name: ProcessDoctor
                 ...
                 State: {rawState}
                 ...
                 Pid: 1
             """;

        var statusFile = CreateStatusFile(id: 1, statusContent);
        var sut = ProcessStatus.Create(statusFile);

        sut.Invoking(processStatus => processStatus.State)
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(1256)]
    public void Should_read_id_properly(uint expectedId)
    {
        // Arrange
        var statusContent =
            $"""
                 Name: ProcessDoctor
                 ...
                 ...
                 ...
                 Pid: {expectedId}
             """;

        var statusFile = CreateStatusFile(expectedId, statusContent);
        var sut = ProcessStatus.Create(statusFile);

        // Act & Assert
        sut.Id
            .Should()
            .Be(expectedId);
    }

    [Theory]
    [InlineData("-512")]
    [InlineData("cmdline")]
    [InlineData("exe623")]
    public void Should_throw_exception_if_id_has_invalid_value(string rawId)
    {
        // Arrange
        var statusContent =
            $"""
                 Name: ProcessDoctor
                 ...
                 ...
                 ...
                 Pid: {rawId}
             """;

        var statusFile = CreateStatusFile(id: 1, statusContent);
        var sut = ProcessStatus.Create(statusFile);

        // Act & Assert
        sut.Invoking(processStatus => processStatus.Id)
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(124)]
    [InlineData(6126)]
    public void Should_read_parent_id_properly(uint expectedParentId)
    {
        // Arrange
        var statusContent =
            $"""
                 Name: ProcessDoctor
                 ...
                 ...
                 ...
                 Pid: 1
                 PPid: {expectedParentId}
             """;

        var statusFile = CreateStatusFile(id: 1, statusContent);
        var sut = ProcessStatus.Create(statusFile);

        // Act & Assert
        sut.ParentId
            .Should()
            .Be(expectedParentId);
    }

    [Fact]
    public void Parent_id_should_be_null_if_it_is_the_same_as_process_id()
    {
        // Arrange
        const string statusContent =
            """
                 Name: ProcessDoctor
                 ...
                 ...
                 ...
                 Pid: 1
                 PPid: 1
             """;

        var statusFile = CreateStatusFile(id: 1, statusContent);
        var sut = ProcessStatus.Create(statusFile);

        // Act & Assert
        sut.ParentId
            .Should()
            .BeNull();
    }

    [Fact]
    public void Parent_id_should_be_null_if_it_is_zero()
    {
        // Arrange
        const string statusContent =
            """
                Name: ProcessDoctor
                ...
                ...
                ...
                Pid: 1
                PPid: 0
            """;

        var statusFile = CreateStatusFile(id: 1, statusContent);
        var sut = ProcessStatus.Create(statusFile);

        // Act & Assert
        sut.ParentId
            .Should()
            .BeNull();
    }

    [Theory]
    [InlineData("-512")]
    [InlineData("cmdline")]
    [InlineData("exe623")]
    public void Should_throw_exception_if_parent_id_has_invalid_value(string rawParentId)
    {
        // Arrange
        var statusContent =
            $"""
                 Name: ProcessDoctor
                 ...
                 ...
                 ...
                 ...
                 PPid: {rawParentId}
             """;

        var statusFile = CreateStatusFile(id: 1, statusContent);
        var sut = ProcessStatus.Create(statusFile);

        // Act & Assert
        sut.Invoking(processStatus => processStatus.ParentId)
            .Should()
            .Throw<InvalidOperationException>();
    }
}
