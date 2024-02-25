using ProcessDoctor.Backend.Core.Enums;

namespace ProcessDoctor.TestFramework;

public sealed class FakeFileSystemEventArgs : FileSystemEventArgs
{
    /// <inheritdoc />
    public FakeFileSystemEventArgs(ObservationTarget observationTarget)
        : base((WatcherChangeTypes)observationTarget, "Fake Directory", "Fake Name")
    { }
}
