using Eshop.Contracts.DTOs;
using FluentValidation;

namespace Eshop.API.Validators
{
    public class PlaceOrderRequestDtoValidator : AbstractValidator<PlaceOrderRequestDto>
    {
        public PlaceOrderRequestDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
        }
    }
}
