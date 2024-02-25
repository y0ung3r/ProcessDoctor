using System.IO.Abstractions;
using System.Text;
using ProcessDoctor.Backend.Linux.Proc.Extensions;

namespace ProcessDoctor.Backend.Linux.Proc;

public sealed class ProcessEntry
{
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

    public string CommandLine
        => _commandLine ??= _processDirectory
            .FileSystem
            .File
            .ReadAllText(Path.Combine(_processDirectory.FullName, ProcPaths.CommandLine.FileName));

    public string ExecutablePath
    {
        get
        {
            if (_executablePath is not null)
                return _executablePath;

            const int bufferSize = 2048;
            var path = Path.Combine(_processDirectory.FullName, ProcPaths.ExecutablePath.FileName);
            var buffer = new byte[bufferSize + 1];
            var count = LibC.ReadLink(path, buffer, bufferSize);

            if (count <= 0)
                throw new InvalidOperationException(
                    $"An error occurred while reading exe file: {LibC.GetLastError()}");

            buffer[count] = 0; // ?

            return _executablePath = Encoding.UTF8.GetString(buffer, 0, count);
        }
    }

    public ProcessStatus Status
        => _status ??= ProcessStatus.Create(
            _processDirectory
                .EnumerateFiles(ProcPaths.Status.FileName)
                .Single());
}
