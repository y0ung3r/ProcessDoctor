using System.IO.Abstractions;
using System.Text;
using ProcessDoctor.Backend.Linux.Proc.Extensions;

namespace ProcessDoctor.Backend.Linux.Proc;

public sealed class ProcessEntry
{
    private const int MaxExecutablePathSize = 2048;

    private readonly IDirectoryInfo _processDirectory;
    private string? _commandLine;
    private string? _executablePath;
    private ProcessStatus? _status;

    public static ProcessEntry Create(IDirectoryInfo processDirectory)
    {
        if (!processDirectory.IsProcess())
            throw new ArgumentException(
                "An error occurred while reading the process folder",
                nameof(processDirectory));

        return new ProcessEntry(processDirectory);
    }

    private ProcessEntry(IDirectoryInfo processDirectory)
        => _processDirectory = processDirectory;

    public string? CommandLine
    {
        get
        {
            if (_commandLine is not null)
                return _commandLine;

            var path = _processDirectory
                .FileSystem
                .Path
                .Combine(_processDirectory.FullName, ProcPaths.CommandLine.FileName);

            var value = _processDirectory
                .FileSystem
                .File
                .ReadAllText(path)
                .Replace('\0', ' ');

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return _commandLine ??= value;
        }
    }

    public string? ExecutablePath
    {
        get
        {
            if (_executablePath is not null)
                return _executablePath;

            var path = _processDirectory
                .FileSystem
                .Path
                .Combine(_processDirectory.FullName, ProcPaths.ExecutablePath.FileName);

            var buffer = new byte[MaxExecutablePathSize + 1];
            var count = LibC.ReadLink(path, buffer, MaxExecutablePathSize);

            if (count <= 0)
                return null;

            buffer[count] = 0x0;

            return _executablePath = Encoding.UTF8.GetString(buffer, index: 0, count);
        }
    }

    public ProcessStatus Status
        => _status ??= ProcessStatus.Create(
            _processDirectory
                .EnumerateFiles(ProcPaths.Status.FileName)
                .Single());
}
