namespace EnterpriseKit.Application.Common.Interfaces;

/// <summary>
/// Marker interface for Commands — state-mutating MediatR requests.
/// Used by <c>TransactionBehaviour</c> to apply a DB transaction.
/// </summary>
public interface ICommand : global::MediatR.IRequest { }

/// <summary>Marker interface for Commands that return a result.</summary>
public interface ICommand<out TResult> : global::MediatR.IRequest<TResult> { }

/// <summary>
/// Marker interface for Queries — read-only MediatR requests.
/// These are explicitly excluded from transaction wrapping.
/// </summary>
public interface IQuery<out TResult> : global::MediatR.IRequest<TResult> { }
