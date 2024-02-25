using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using ProcessDoctor.Backend.Core;
using ProcessDoctor.Backend.Core.Interfaces;
using ProcessDoctor.Backend.Windows.WMI;

namespace ProcessDoctor;

internal sealed class ProcessMonitorFactory : IProcessMonitorFactory
{
    /// <inheritdoc />
    public IProcessMonitor Create(Lifetime lifetime)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new ProcessMonitor(
                Log.GetLog<ProcessMonitor>(),
                new Backend.Windows.ProcessProvider(
                    lifetime,
                    Log.GetLog<Backend.Windows.ProcessProvider>(),
                    new ManagementEventWatcherAdapterFactory()));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new ProcessMonitor(
                Log.GetLog<ProcessMonitor>(),
                new Backend.Linux.ProcessProvider(
                    lifetime,
                    Log.GetLog<Backend.Linux.ProcessProvider>(),
                    new FileSystem()));

        throw new NotSupportedException(
            $"Current operating system is not supported ({RuntimeInformation.OSDescription})");
    }
}
