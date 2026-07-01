namespace EnterpriseKit.Application.UnitTests.Orders.Commands;

using EnterpriseKit.Application.Orders.Commands.PlaceOrder;
using EnterpriseKit.Domain.Interfaces.Repositories;
using EnterpriseKit.Domain.Orders;
using FluentAssertions;
using Moq;
using Xunit;

public sealed class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly PlaceOrderCommandHandler _handler;

    public PlaceOrderCommandHandlerTests()
    {
        _handler = new PlaceOrderCommandHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewOrderId()
    {
        var command = new PlaceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Lines:
            [
                new OrderLineInput(Guid.NewGuid(), 2, 49.99m, "USD")
            ]);

        _orderRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsAddAsyncOnRepository()
    {
        var command = new PlaceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Lines:
            [
                new OrderLineInput(Guid.NewGuid(), 1, 100m, "USD")
            ]);

        await _handler.Handle(command, CancellationToken.None);

        _orderRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
