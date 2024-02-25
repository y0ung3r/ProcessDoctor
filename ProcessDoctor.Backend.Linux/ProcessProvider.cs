using System.IO.Abstractions;
using System.Reactive.Linq;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using ProcessDoctor.Backend.Core;
using ProcessDoctor.Backend.Core.Enums;
using ProcessDoctor.Backend.Core.Interfaces;
using ProcessDoctor.Backend.Linux.Extensions;
using ProcessDoctor.Backend.Linux.Proc;
using ProcessDoctor.Backend.Linux.Proc.Extensions;

namespace ProcessDoctor.Backend.Linux;

public sealed class ProcessProvider(Lifetime lifetime, ILog logger, IFileSystem fileSystem) : IProcessProvider
{
    /// <inheritdoc />
    public IObservable<SystemProcess> ObserveProcesses(ObservationTarget targetState)
    {
        var lifetimeScope = lifetime.CreateNested();
        var watcher = fileSystem.FileSystemWatcher.New(ProcPaths.Path);

        lifetimeScope
            .Lifetime
            .AddDispose(watcher);

        lifetimeScope
            .Lifetime
            .OnTermination(() =>
            {
                logger.Info("File system event watcher has been stopped");
                logger.Info("File system event watcher has been disposed");
            });

        logger.Info("File system event watcher has been started");

        return watcher
            .AsObservable(targetState)
            .Select(arrivedEvent => fileSystem.DirectoryInfo.New(arrivedEvent.EventArgs.FullPath))
            .Where(DirectoryInfoExtensions.IsProcess)
            .Select(directory =>
                LinuxProcess.Create(
                    ProcessEntry.Create(directory)))
            .Finally(lifetimeScope.Terminate)
            .Catch((Exception exception) =>
            {
                logger.Error(exception, "An error occurred while processing an event received from the file system");

                return ObserveProcesses(targetState);
            });
    }

    /// <inheritdoc />
    public IProcessListSnapshot CreateSnapshot()
        => new ProcessListSnapshot(
            Log.GetLog<ProcessListSnapshot>(),
            fileSystem);
}
