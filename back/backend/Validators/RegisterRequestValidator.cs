using FluentValidation;
using backend.Dtos;

namespace backend.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
            RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(256);
        }
    }
}