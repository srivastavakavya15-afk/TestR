using FluentAssertions;
using TestR.Application.Users;

namespace TestR.Application.Tests.Users;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    private static CreateUserRequest Valid() =>
        new("Ada Lovelace", 36, "London", "Greater London", "WC1E");

    [Fact]
    public void Validate_WithValidRequest_Passes()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void Validate_WithTooShortName_FailsOnName(string name)
    {
        var result = _validator.Validate(Valid() with { Name = name });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    public void Validate_WithAgeOutOfRange_FailsOnAge(int age)
    {
        var result = _validator.Validate(Valid() with { Age = age });

        result.Errors.Should().Contain(e => e.PropertyName == "Age");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    public void Validate_WithPincodeOutsideBounds_FailsOnPincode(string pincode)
    {
        var result = _validator.Validate(Valid() with { Pincode = pincode });

        result.Errors.Should().Contain(e => e.PropertyName == "Pincode");
    }

    [Fact]
    public void Validate_WithEveryFieldBlank_ReportsEachField()
    {
        var result = _validator.Validate(new CreateUserRequest("", -1, "", "", ""));

        result.Errors.Select(e => e.PropertyName).Distinct()
            .Should().BeEquivalentTo("Name", "Age", "City", "State", "Pincode");
    }
}

public class UpdateUserRequestValidatorTests
{
    [Fact]
    public void Validate_SharesTheSameRulesAsCreate()
    {
        var validator = new UpdateUserRequestValidator();

        validator.Validate(new UpdateUserRequest("Ada Lovelace", 36, "London", "Greater London", "WC1E"))
            .IsValid.Should().BeTrue();
        validator.Validate(new UpdateUserRequest("A", 200, "", "", "1"))
            .IsValid.Should().BeFalse();
    }
}
