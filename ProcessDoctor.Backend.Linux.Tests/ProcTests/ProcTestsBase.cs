using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using ProcessDoctor.Backend.Linux.Proc;

namespace ProcessDoctor.Backend.Linux.Tests.ProcTests;

public abstract class ProcTestsBase
{
    private readonly MockFileSystem _fileSystem = new();

    protected IDirectoryInfo CreateTestProcess(uint id)
    {
        var processPath = _fileSystem.Path.Combine(ProcPaths.Path, id.ToString());

        var directoryInfo = _fileSystem.DirectoryInfo.New(processPath);
        _fileSystem.AddDirectory(directoryInfo);

        return directoryInfo;
    }

    protected IFileInfo CreateStatusFile(uint id, string content)
        => CreateProcessFile(id, ProcPaths.Status.FileName, new MockFileData(content));

    protected IFileInfo CreateCommandLineFile(uint id, string content)
        => CreateProcessFile(id, ProcPaths.CommandLine.FileName, new MockFileData(content));

    private IFileInfo CreateProcessFile(uint id, string fileName, MockFileData content)
    {
        var path = _fileSystem.Path.Combine(
            ProcPaths.Path,
            id.ToString(),
            fileName);

        var fileInfo = _fileSystem.FileInfo.New(path);
        _fileSystem.AddFile(fileInfo, content);

        return fileInfo;
    }
}
