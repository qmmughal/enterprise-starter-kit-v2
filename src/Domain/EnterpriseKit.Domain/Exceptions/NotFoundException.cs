namespace EnterpriseKit.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity cannot be found.
/// Maps to HTTP 404 Not Found in the API layer.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base("NOT_FOUND", $"Entity '{entityName}' with key '{key}' was not found.")
    { }

    public NotFoundException(string message)
        : base("NOT_FOUND", message)
    { }
}
