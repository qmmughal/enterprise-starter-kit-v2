namespace EnterpriseKit.Application.Orders.Commands.PlaceOrder;

using FluentValidation;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one order line is required.")
            .Must(l => l.Count <= 100).WithMessage("An order cannot have more than 100 lines.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(10_000).WithMessage("Quantity cannot exceed 10,000.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThan(0).WithMessage("UnitPrice must be greater than zero.");

            line.RuleFor(l => l.Currency)
                .NotEmpty()
                .Length(3).WithMessage("Currency must be a 3-letter ISO 4217 code.");
        });
    }
}
