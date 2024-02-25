using System.Runtime.InteropServices;

namespace ProcessDoctor.Backend.Linux.Proc;

internal static class LibC
{
    private const string Name = "libc";

    [DllImport(Name, EntryPoint = "readlink", SetLastError = true)]
    internal static extern int ReadLink(string path, byte[] buffer, int bufferSize);

    internal static string? GetLastError()
        /*
         * The DllImportAttribute provides a SetLastError property
         * so the runtime knows to immediately capture the last error and
         * store it in a place that the managed code can read using Marshal.GetLastWin32Error.
         */
        => Marshal.PtrToStringAnsi(StrError(Marshal.GetLastWin32Error()));

    [DllImport(Name, EntryPoint = "strerror", SetLastError = false)]
    private static extern IntPtr StrError(int errorCode);
}
