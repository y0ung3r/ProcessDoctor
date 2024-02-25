using System.IO.Abstractions;
using JetBrains.Diagnostics;
using ProcessDoctor.Backend.Core;
using ProcessDoctor.Backend.Core.Interfaces;
using ProcessDoctor.Backend.Linux.Proc;
using ProcessDoctor.Backend.Linux.Proc.Extensions;

namespace ProcessDoctor.Backend.Linux;

public sealed class ProcessListSnapshot(ILog logger, IFileSystem fileSystem) : IProcessListSnapshot
{
    /// <inheritdoc />
    public IEnumerable<SystemProcess> EnumerateProcesses()
        => logger.Catch(() =>
            fileSystem
                .Directory
                .EnumerateDirectories(ProcPaths.Path)
                .Select(directory => fileSystem.DirectoryInfo.New(directory))
                .Where(DirectoryInfoExtensions.IsProcess)
                .Select(ProcessEntry.Create)
                .Select(LinuxProcess.Create))
                    ?? Enumerable.Empty<SystemProcess>();

    /// <inheritdoc />
    public void Dispose()
    { }
}
