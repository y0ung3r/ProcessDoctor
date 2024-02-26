using JetBrains.Lifetimes;

namespace ProcessDoctor.Backend.Core.Interfaces;

public interface IProcessProviderFactory
{
    IProcessProvider Create(Lifetime lifetime);
}
