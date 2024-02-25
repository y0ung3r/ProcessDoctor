using ProcessDoctor.Backend.Core;
using ProcessDoctor.Backend.Linux.Proc;

namespace ProcessDoctor.Backend.Linux;

internal sealed record LinuxProcess : SystemProcess
{
    public static LinuxProcess Create(ProcessEntry processEntry)
        => new(
            processEntry.Status.Id,
            processEntry.Status.ParentId,
            processEntry.Status.Name,
            processEntry.CommandLine,
            processEntry.ExecutablePath);

    /// <inheritdoc />
    private LinuxProcess(uint id, uint? parentId, string name, string? commandLine, string? executablePath)
        : base(id, parentId, name, commandLine, executablePath)
    { }
}
