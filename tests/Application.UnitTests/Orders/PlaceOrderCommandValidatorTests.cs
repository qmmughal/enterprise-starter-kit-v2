namespace EnterpriseKit.Application.UnitTests.Orders.Validators;

using EnterpriseKit.Application.Orders.Commands.PlaceOrder;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

public sealed class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var cmd = new PlaceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Lines: [new OrderLineInput(Guid.NewGuid(), 1, 10m, "USD")]);

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyCustomerId_FailsValidation()
    {
        var cmd = new PlaceOrderCommand(
            CustomerId: Guid.Empty,
            Lines: [new OrderLineInput(Guid.NewGuid(), 1, 10m, "USD")]);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_EmptyLines_FailsValidation()
    {
        var cmd = new PlaceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Lines: []);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Validate_ZeroQuantity_FailsValidation()
    {
        var cmd = new PlaceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Lines: [new OrderLineInput(Guid.NewGuid(), 0, 10m, "USD")]);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("Lines[0].Quantity");
    }

    [Fact]
    public void Validate_NegativeUnitPrice_FailsValidation()
    {
        var cmd = new PlaceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Lines: [new OrderLineInput(Guid.NewGuid(), 1, -5m, "USD")]);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("Lines[0].UnitPrice");
    }

    [Fact]
    public void Validate_InvalidCurrencyLength_FailsValidation()
    {
        var cmd = new PlaceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Lines: [new OrderLineInput(Guid.NewGuid(), 1, 10m, "US")]);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor("Lines[0].Currency");
    }
}
