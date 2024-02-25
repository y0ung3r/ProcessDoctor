using JetBrains.Lifetimes;

namespace ProcessDoctor.Backend.Core.Interfaces;

public interface IProcessMonitorFactory
{
    IProcessMonitor Create(Lifetime lifetime);
}
