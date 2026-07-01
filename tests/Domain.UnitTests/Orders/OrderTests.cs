namespace EnterpriseKit.Domain.UnitTests.Orders;

using EnterpriseKit.Domain.Exceptions;
using EnterpriseKit.Domain.Orders;
using EnterpriseKit.Domain.Orders.Events;
using FluentAssertions;
using Xunit;

public sealed class OrderTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly List<(Guid ProductId, int Quantity, decimal UnitPrice)> ValidLines =
    [
        (Guid.NewGuid(), 2, 19.99m),
        (Guid.NewGuid(), 1, 49.99m)
    ];

    // ── Place ──────────────────────────────────────────────────────────────

    [Fact]
    public void Place_WithValidLines_CreatesOrderInPendingStatus()
    {
        var order = Order.Place(CustomerId, ValidLines);

        order.Status.Should().Be(OrderStatus.Pending);
        order.CustomerId.Should().Be(CustomerId);
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Place_WithValidLines_CalculatesTotalCorrectly()
    {
        var order = Order.Place(CustomerId, ValidLines);

        // 2 × 19.99 + 1 × 49.99 = 89.97
        order.Total.Amount.Should().Be(89.97m);
        order.Total.Currency.Should().Be("USD");
    }

    [Fact]
    public void Place_WithValidLines_RaisesOrderPlacedEvent()
    {
        var order = Order.Place(CustomerId, ValidLines);

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderPlacedEvent>()
            .Which.CustomerId.Should().Be(CustomerId);
    }

    [Fact]
    public void Place_WithEmptyLines_ThrowsDomainException()
    {
        var act = () => Order.Place(CustomerId, []);

        act.Should().Throw<DomainException>()
            .WithMessage("*at least one item*");
    }

    [Fact]
    public void Place_AssignsNewGuidId()
    {
        var order = Order.Place(CustomerId, ValidLines);
        order.Id.Should().NotBeEmpty();
    }

    // ── Cancel ─────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_PendingOrder_Succeeds()
    {
        var order = Order.Place(CustomerId, ValidLines);

        order.Cancel("Customer changed mind");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Customer changed mind");
    }

    [Fact]
    public void Cancel_PendingOrder_RaisesCancelledEvent()
    {
        var order = Order.Place(CustomerId, ValidLines);
        order.ClearDomainEvents();

        order.Cancel("Duplicate order");

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCancelledEvent>();
    }

    [Fact]
    public void Cancel_ShippedOrder_ThrowsDomainException()
    {
        var order = Order.Place(CustomerId, ValidLines);
        order.Confirm();
        order.Ship();

        var act = () => order.Cancel("Too late");

        act.Should().Throw<DomainException>()
            .Where(e => e.Code == "ORDER_CANNOT_CANCEL");
    }

    [Fact]
    public void Cancel_AlreadyCancelledOrder_ThrowsDomainException()
    {
        var order = Order.Place(CustomerId, ValidLines);
        order.Cancel("First cancel");

        var act = () => order.Cancel("Second cancel");

        act.Should().Throw<DomainException>()
            .Where(e => e.Code == "ORDER_ALREADY_CANCELLED");
    }

    // ── Confirm ────────────────────────────────────────────────────────────

    [Fact]
    public void Confirm_PendingOrder_ChangesStatusToConfirmed()
    {
        var order = Order.Place(CustomerId, ValidLines);

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_NonPendingOrder_ThrowsDomainException()
    {
        var order = Order.Place(CustomerId, ValidLines);
        order.Confirm();

        var act = () => order.Confirm(); // already confirmed

        act.Should().Throw<DomainException>()
            .Where(e => e.Code == "ORDER_NOT_PENDING");
    }

    // ── Money ──────────────────────────────────────────────────────────────

    [Fact]
    public void Money_NegativeAmount_ThrowsArgumentException()
    {
        var act = () => Money.Of(-1, "USD");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Money_DifferentCurrencyAdd_ThrowsInvalidOperationException()
    {
        var usd = Money.Of(10, "USD");
        var eur = Money.Of(10, "EUR");

        var act = () => usd.Add(eur);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Money_EqualityIsStructural()
    {
        var a = Money.Of(100, "USD");
        var b = Money.Of(100, "USD");
        var c = Money.Of(200, "USD");

        a.Should().Be(b);
        a.Should().NotBe(c);
    }
}
