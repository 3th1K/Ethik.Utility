using System.Reflection;

namespace Ethik.Utility.CQRS;

public class CQRSConfiguration
{
    internal List<Assembly> Assemblies { get; } = new();

    public CQRSConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
        return this;
    }
}