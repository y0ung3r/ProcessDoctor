using System.IO.Abstractions;
using JetBrains.Lifetimes;
using Microsoft.Reactive.Testing;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using ProcessDoctor.Backend.Core;
using ProcessDoctor.Backend.Core.Enums;
using ProcessDoctor.TestFramework;
using ProcessDoctor.TestFramework.Logging;
using Xunit.Abstractions;

namespace ProcessDoctor.Backend.Linux.Tests;

public sealed class ProcessProviderTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(ObservationTarget.Launched)]
    [InlineData(ObservationTarget.Terminated)]
    public void Should_restart_observing_if_error_occurrs(ObservationTarget observationTarget)
    {
        // Arrange
        using var logger = new ThrowingLoggerAdapter(nameof(ProcessProviderTests), output);
        var fileSystem = Substitute.For<IFileSystem>();
        var directoryInfoFactory = Substitute.For<IDirectoryInfoFactory>();

        directoryInfoFactory
            .New(Arg.Any<string>())
            .Returns(
                _ => throw new FakeException(),
                _ => Substitute.For<IDirectoryInfo>());

        fileSystem
            .DirectoryInfo
            .Returns(directoryInfoFactory);

        var watcherFactory = Substitute.For<IFileSystemWatcherFactory>();
        var watcher = Substitute.For<IFileSystemWatcher>();

        watcherFactory
            .New(Arg.Any<string>())
            .Returns(watcher);

        fileSystem
            .FileSystemWatcher
            .Returns(watcherFactory);

        var testObserver = new TestScheduler()
            .CreateObserver<SystemProcess>();

        // Act
        new ProcessProvider(Lifetime.Eternal, logger, fileSystem)
            .ObserveProcesses(observationTarget)
            .Subscribe(testObserver);

        watcher.Created += Raise.Event<FileSystemEventHandler>(
            new FakeFileSystemEventArgs(observationTarget));

        watcher.Deleted += Raise.Event<FileSystemEventHandler>(
            new FakeFileSystemEventArgs(observationTarget));

        // Assert
        watcherFactory
            .Received(Quantity.Exactly(number: 2))
            .New(Arg.Any<string>());
    }

    [Theory]
    [InlineData(ObservationTarget.Launched)]
    [InlineData(ObservationTarget.Terminated)]
    public void Should_dispose_old_watcher_if_error_occurrs(ObservationTarget observationTarget)
    {
        // Arrange
        using var logger = new ThrowingLoggerAdapter(nameof(ProcessProviderTests), output);
        var fileSystem = Substitute.For<IFileSystem>();
        var directoryInfoFactory = Substitute.For<IDirectoryInfoFactory>();

        directoryInfoFactory
            .New(Arg.Any<string>())
            .Returns(
                _ => throw new FakeException(),
                _ => Substitute.For<IDirectoryInfo>());

        fileSystem
            .DirectoryInfo
            .Returns(directoryInfoFactory);

        var watcherFactory = Substitute.For<IFileSystemWatcherFactory>();
        var watcher = Substitute.For<IFileSystemWatcher>();

        watcherFactory
            .New(Arg.Any<string>())
            .Returns(watcher);

        fileSystem
            .FileSystemWatcher
            .Returns(watcherFactory);

        var testObserver = new TestScheduler()
            .CreateObserver<SystemProcess>();

        // Act
        new ProcessProvider(Lifetime.Eternal, logger, fileSystem)
            .ObserveProcesses(observationTarget)
            .Subscribe(testObserver);

        watcher.Created +=
            Raise.Event<FileSystemEventHandler>(
                new FileSystemEventArgs(WatcherChangeTypes.Created, "directory", "name"));

        watcher.Deleted +=
            Raise.Event<FileSystemEventHandler>(
                new FileSystemEventArgs(WatcherChangeTypes.Deleted, "directory", "name"));

        // Assert
        watcher
            .Received(Quantity.Exactly(number: 1))
            .Dispose();
    }

    [Theory]
    [InlineData(ObservationTarget.Launched)]
    [InlineData(ObservationTarget.Terminated)]
    public void Should_dispose_old_watcher_if_subscription_is_disposed(ObservationTarget observationTarget)
    {
        // Arrange
        using var logger = new ThrowingLoggerAdapter(nameof(ProcessProviderTests), output);
        var fileSystem = Substitute.For<IFileSystem>();
        var watcherFactory = Substitute.For<IFileSystemWatcherFactory>();
        var watcher = Substitute.For<IFileSystemWatcher>();

        watcherFactory
            .New(Arg.Any<string>())
            .Returns(watcher);

        fileSystem
            .FileSystemWatcher
            .Returns(watcherFactory);

        var testObserver = new TestScheduler()
            .CreateObserver<SystemProcess>();

        var subscription = new ProcessProvider(Lifetime.Eternal, logger, fileSystem)
            .ObserveProcesses(observationTarget)
            .Subscribe(testObserver);

        // Act
        subscription.Dispose();

        // Assert
        watcher
            .Received(Quantity.Exactly(number: 1))
            .Dispose();
    }

    [Theory]
    [InlineData(ObservationTarget.Launched)]
    [InlineData(ObservationTarget.Terminated)]
    public void Should_dispose_old_watcher_if_lifetime_is_terminated(ObservationTarget observationTarget)
    {
        // Arrange
        using var logger = new ThrowingLoggerAdapter(nameof(ProcessProviderTests), output);
        var fileSystem = Substitute.For<IFileSystem>();
        var watcherFactory = Substitute.For<IFileSystemWatcherFactory>();
        var watcher = Substitute.For<IFileSystemWatcher>();

        watcherFactory
            .New(Arg.Any<string>())
            .Returns(watcher);

        fileSystem
            .FileSystemWatcher
            .Returns(watcherFactory);

        var lifetimeScope = new LifetimeDefinition();

        var testObserver = new TestScheduler()
            .CreateObserver<SystemProcess>();

        new ProcessProvider(lifetimeScope.Lifetime, logger, fileSystem)
            .ObserveProcesses(observationTarget)
            .Subscribe(testObserver);

        // Act
        lifetimeScope.Terminate();

        // Assert
        watcher
            .Received(Quantity.Exactly(number: 1))
            .Dispose();
    }
}
