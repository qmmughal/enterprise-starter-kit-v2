namespace EnterpriseKit.Application.Orders.Commands.CancelOrder;

using EnterpriseKit.Application.Common.Interfaces;

/// <summary>Command: Cancel an existing order.</summary>
public sealed record CancelOrderCommand(Guid OrderId, string Reason) : ICommand;
