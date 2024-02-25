using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using ProcessDoctor.Backend.Linux.Proc.Enums;

namespace ProcessDoctor.Backend.Linux.Proc;

public sealed class ProcessStatus
{
    private const int MinLineIndex = 1;
    private const int MaxLineIndex = 51;
    private const char Separator = ':';

    private readonly IFileInfo _statusFile;
    private string? _name;
    private ProcessState? _state;
    private uint? _id;
    private uint? _parentId;

    public static ProcessStatus Create(IFileInfo statusFile)
    {
        if (statusFile.Name != ProcPaths.Status.FileName)
            throw new ArgumentException(
                "An error occurred while reading the status file",
                nameof(statusFile));

        return new ProcessStatus(statusFile);
    }

    private ProcessStatus(IFileInfo statusFile)
        => _statusFile = statusFile;

    public string Name
        => _name ??= ReadValue(lineIndex: 1);

    public ProcessState State
    {
        get
        {
            if (_state is not null)
                return _state.Value;

            var raw = ReadValue(lineIndex: 3)
                .First()
                .ToString();

            var state = raw switch
            {
                "R" => ProcessState.Running,
                "S" => ProcessState.Sleeping,
                "D" => ProcessState.UninterruptibleWait,
                "Z" => ProcessState.Zombie,
                "T" => ProcessState.TracedOrStopped,
                _ => default(ProcessState?)
            };

            if (state is null)
                ThrowIfPropertyValueIsInvalid(lineIndex: 3);

            return _state ??= state.Value;
        }
    }

    public uint Id
    {
        get
        {
            if (_id is not null)
                return _id.Value;

            var rawValue = ReadValue(lineIndex: 5);

            if (!uint.TryParse(rawValue, out var id))
                ThrowIfPropertyValueIsInvalid(lineIndex: 5);

            return _id ??= id;
        }
    }

    public uint? ParentId
    {
        get
        {
            if (_parentId is not null)
                return _parentId;

            var rawValue = ReadValue(lineIndex: 6);

            if (!uint.TryParse(rawValue, out var parentId))
                ThrowIfPropertyValueIsInvalid(lineIndex: 6);

            if (parentId == 0 || Id == parentId)
                return null;

            return _parentId ??= parentId;
        }
    }

    private string ReadValue(int lineIndex)
    {
        if (lineIndex is < MinLineIndex or > MaxLineIndex)
            throw new ArgumentException(
                $"To read properties of the status file, you must specify a line number from {MinLineIndex} to {MaxLineIndex}",
                nameof(lineIndex));

        var line = _statusFile
            .FileSystem
            .File
            .ReadLines(_statusFile.FullName)
            .Skip(lineIndex - 1)
            .FirstOrDefault();

        if (line is null)
            ThrowIfPropertyValueIsInvalid(lineIndex);

        var separatorIndex = line.IndexOf(Separator, StringComparison.Ordinal);

        if (separatorIndex is -1)
            ThrowIfPropertyValueIsInvalid(lineIndex);

        var value = line.Substring(
            separatorIndex + 1,
            line.Length - separatorIndex - 1);

        return value.Trim();
    }

    [DoesNotReturn]
    private static void ThrowIfPropertyValueIsInvalid(int lineIndex)
        => throw new InvalidOperationException(
            $"An error occurred while reading property under {lineIndex} line index");
}
