namespace EnterpriseKit.Application.Orders.Queries.GetOrderById;

using EnterpriseKit.Application.Common.Interfaces;

/// <summary>Query: Retrieve a single order by ID.</summary>
public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDetailDto>;
