using FluentValidation;
using TestR.Domain.Users;

namespace TestR.Application.Users;

public abstract class UserWriteRequestValidator<T> : AbstractValidator<T>
    where T : IUserWriteRequest
{
    protected UserWriteRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(User.NameMinLength, User.NameMaxLength)
            .WithMessage($"Name must be between {User.NameMinLength} and {User.NameMaxLength} characters.");

        RuleFor(x => x.Age)
            .InclusiveBetween(User.AgeMin, User.AgeMax)
            .WithMessage($"Age must be between {User.AgeMin} and {User.AgeMax}.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(User.CityMaxLength);

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(User.StateMaxLength);

        RuleFor(x => x.Pincode)
            .NotEmpty().WithMessage("Pincode is required.")
            .Length(User.PincodeMinLength, User.PincodeMaxLength)
            .WithMessage($"Pincode must be between {User.PincodeMinLength} and {User.PincodeMaxLength} characters.");
    }
}

public sealed class CreateUserRequestValidator : UserWriteRequestValidator<CreateUserRequest>;

public sealed class UpdateUserRequestValidator : UserWriteRequestValidator<UpdateUserRequest>;
