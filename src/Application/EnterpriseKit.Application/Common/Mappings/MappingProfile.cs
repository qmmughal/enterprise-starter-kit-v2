namespace EnterpriseKit.Application.Common.Mappings;

using AutoMapper;
using System.Reflection;

/// <summary>
/// Scans the Application assembly for all types that implement
/// <see cref="IMapFrom{T}"/> and registers their mappings automatically.
/// </summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);
        const string mappingMethodName = nameof(IMapFrom<object>.Mapping);

        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType));

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod(mappingMethodName)
                ?? type.GetInterface(mapFromType.Name)!
                       .GetMethod(mappingMethodName);

            methodInfo?.Invoke(instance, [this]);
        }
    }
}
