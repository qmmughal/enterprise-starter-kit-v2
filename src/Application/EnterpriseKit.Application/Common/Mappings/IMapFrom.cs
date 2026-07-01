namespace EnterpriseKit.Application.Common.Mappings;

using AutoMapper;

/// <summary>
/// Convenience interface. Any DTO that implements <c>IMapFrom&lt;T&gt;</c>
/// gets a default AutoMapper mapping from T automatically applied
/// via <see cref="MappingProfile"/>.
/// </summary>
public interface IMapFrom<T>
{
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}
