using JetBrains.Diagnostics;
using System;
using System.Runtime.InteropServices;
using JetBrains.Lifetimes;
using ProcessDoctor.Backend.Core.Interfaces;
#if WINDOWS
using ProcessDoctor.Backend.Windows.WMI;
#elif LINUX
using System.IO.Abstractions;
#else
// macOS
#endif

namespace ProcessDoctor;

internal sealed class ProcessProviderFactory : IProcessProviderFactory
{
    /// <inheritdoc />
    public IProcessProvider Create(Lifetime lifetime)
    {
#if WINDOWS
        return new Backend.Windows.ProcessProvider(
            lifetime,
            Log.GetLog<Backend.Windows.ProcessProvider>(),
            new ManagementEventWatcherAdapterFactory());

#elif LINUX
        return new Backend.Linux.ProcessProvider(
            lifetime,
            Log.GetLog<Backend.Linux.ProcessProvider>(),
            new FileSystem());
#else
        throw new NotSupportedException(
            $"Current operating system is not supported ({RuntimeInformation.OSDescription})");
#endif
    }
}
