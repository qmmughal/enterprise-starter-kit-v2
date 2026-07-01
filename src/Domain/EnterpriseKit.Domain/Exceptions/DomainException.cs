namespace EnterpriseKit.Domain.Exceptions;

/// <summary>
/// Base class for all domain rule violations.
/// These map to HTTP 422 Unprocessable Entity in the API layer.
/// </summary>
public class DomainException : Exception
{
    /// <summary>Machine-readable error code (e.g., "ORDER_CANNOT_CANCEL").</summary>
    public string Code { get; }

    public DomainException(string code, string message)
        : base(message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    public DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }
}
