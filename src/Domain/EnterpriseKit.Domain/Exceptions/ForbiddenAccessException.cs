namespace EnterpriseKit.Domain.Exceptions;

/// <summary>
/// Thrown when a business action is forbidden given the caller's identity
/// or the entity's current state. Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenAccessException : DomainException
{
    public ForbiddenAccessException()
        : base("FORBIDDEN", "You do not have permission to perform this action.")
    { }

    public ForbiddenAccessException(string message)
        : base("FORBIDDEN", message)
    { }
}
