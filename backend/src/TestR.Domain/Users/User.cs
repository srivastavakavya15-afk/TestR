namespace TestR.Domain.Users;

public sealed class User
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;
    public const int AgeMin = 0;
    public const int AgeMax = 120;
    public const int CityMaxLength = 100;
    public const int StateMaxLength = 100;
    public const int PincodeMinLength = 4;
    public const int PincodeMaxLength = 10;

    private User()
    {

    }

    public User(string name, int age, string city, string state, string pincode)
    {
        Id = SequentialGuid.Create();
        Name = NormaliseName(name);
        Age = NormaliseAge(age);
        City = NormaliseRequiredText(city, nameof(City), CityMaxLength);
        State = NormaliseRequiredText(state, nameof(State), StateMaxLength);
        Pincode = NormalisePincode(pincode);
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public int Age { get; private set; }
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string Pincode { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public void Update(string name, int age, string city, string state, string pincode)
    {
        Name = NormaliseName(name);
        Age = NormaliseAge(age);
        City = NormaliseRequiredText(city, nameof(City), CityMaxLength);
        State = NormaliseRequiredText(state, nameof(State), StateMaxLength);
        Pincode = NormalisePincode(pincode);
    }

    private static string NormaliseName(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length is < NameMinLength or > NameMaxLength)
        {
            throw new DomainException(
                $"Name must be between {NameMinLength} and {NameMaxLength} characters.");
        }

        return trimmed;
    }

    private static int NormaliseAge(int age)
    {
        if (age is < AgeMin or > AgeMax)
        {
            throw new DomainException($"Age must be between {AgeMin} and {AgeMax}.");
        }

        return age;
    }

    private static string NormaliseRequiredText(string value, string field, int maxLength)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new DomainException($"{field} is required.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{field} must be at most {maxLength} characters.");
        }

        return trimmed;
    }

    private static string NormalisePincode(string pincode)
    {
        var trimmed = (pincode ?? string.Empty).Trim();
        if (trimmed.Length is < PincodeMinLength or > PincodeMaxLength)
        {
            throw new DomainException(
                $"Pincode must be between {PincodeMinLength} and {PincodeMaxLength} characters.");
        }

        return trimmed;
    }
}
