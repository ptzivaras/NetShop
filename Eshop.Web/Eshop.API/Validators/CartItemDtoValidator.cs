using Eshop.Contracts.DTOs;
using FluentValidation;

namespace Eshop.API.Validators
{
    public class CartItemDtoValidator : AbstractValidator<CartItemDto>
    {
        public CartItemDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Valid product must be selected");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1");
        }
    }
}
