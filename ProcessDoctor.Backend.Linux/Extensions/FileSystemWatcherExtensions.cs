using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Linq;
using ProcessDoctor.Backend.Core.Enums;

namespace ProcessDoctor.Backend.Linux.Extensions;

public static class FileSystemWatcherExtensions
{
    public static IObservable<EventPattern<FileSystemEventArgs>> AsObservable(this IFileSystemWatcher watcher, ObservationTarget targetState)
        => targetState switch
        {
            ObservationTarget.Launched => Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    eventHandler => watcher.Created += eventHandler,
                    eventHandler => watcher.Created -= eventHandler),

            ObservationTarget.Terminated => Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    eventHandler => watcher.Deleted += eventHandler,
                    eventHandler => watcher.Deleted -= eventHandler),

            _ => throw new ArgumentOutOfRangeException(
                nameof(targetState),
                targetState,
                $"Process state {targetState} is not supported")
        };
}
