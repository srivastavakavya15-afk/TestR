using FluentAssertions;
using TestR.Domain;
using TestR.Domain.Users;

namespace TestR.Application.Tests.Users;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidValues_TrimsTextAndAssignsId()
    {
        var user = new User("  Ada Lovelace  ", 36, " London ", " Greater London ", " WC1E ");

        user.Id.Should().NotBe(Guid.Empty);
        user.Name.Should().Be("Ada Lovelace");
        user.Age.Should().Be(36);
        user.City.Should().Be("London");
        user.State.Should().Be("Greater London");
        user.Pincode.Should().Be("WC1E");
        user.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNameShorterThanTwoCharacters_Throws(string name)
    {
        var act = () => new User(name, 30, "London", "Greater London", "WC1E");

        act.Should().Throw<DomainException>().WithMessage("*between 2 and 100*");
    }

    [Fact]
    public void Constructor_WithNameLongerThanHundredCharacters_Throws()
    {
        var act = () => new User(new string('a', 101), 30, "London", "Greater London", "WC1E");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    public void Constructor_WithAgeOutOfRange_Throws(int age)
    {
        var act = () => new User("Ada Lovelace", age, "London", "Greater London", "WC1E");

        act.Should().Throw<DomainException>().WithMessage("*between 0 and 120*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    public void Constructor_WithAgeAtBoundary_Succeeds(int age)
    {
        var user = new User("Ada Lovelace", age, "London", "Greater London", "WC1E");

        user.Age.Should().Be(age);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    public void Constructor_WithPincodeOutsideLengthBounds_Throws(string pincode)
    {
        var act = () => new User("Ada Lovelace", 30, "London", "Greater London", pincode);

        act.Should().Throw<DomainException>().WithMessage("*between 4 and 10*");
    }

    [Fact]
    public void Constructor_WithBlankCityOrState_Throws()
    {
        var blankCity = () => new User("Ada Lovelace", 30, "  ", "Greater London", "WC1E");
        var blankState = () => new User("Ada Lovelace", 30, "London", "  ", "WC1E");

        blankCity.Should().Throw<DomainException>().WithMessage("*City is required*");
        blankState.Should().Throw<DomainException>().WithMessage("*State is required*");
    }

    [Fact]
    public void Update_WithValidValues_ChangesFieldsButKeepsIdAndCreatedAt()
    {
        var user = new User("Ada Lovelace", 36, "London", "Greater London", "WC1E");
        var originalId = user.Id;
        var originalCreatedAt = user.CreatedAtUtc;

        user.Update("Grace Hopper", 45, "Arlington", "VA", "22201");

        user.Id.Should().Be(originalId);
        user.CreatedAtUtc.Should().Be(originalCreatedAt);
        user.Name.Should().Be("Grace Hopper");
        user.Age.Should().Be(45);
        user.City.Should().Be("Arlington");
        user.State.Should().Be("VA");
        user.Pincode.Should().Be("22201");
    }

    [Fact]
    public void Update_WithInvalidValues_ThrowsAndLeavesTheEntityUnchanged()
    {
        var user = new User("Ada Lovelace", 36, "London", "Greater London", "WC1E");

        var act = () => user.Update("X", 200, "", "", "1");

        act.Should().Throw<DomainException>();
        user.Name.Should().Be("Ada Lovelace");
        user.Age.Should().Be(36);
    }
}

public class SequentialGuidTests
{
    [Fact]
    public void Create_ProducesVersion7Guids()
    {
        var bytes = SequentialGuid.Create().ToByteArray();

        (bytes[7] >> 4).Should().Be(7);
    }

    [Fact]
    public void Create_ProducesValuesThatSortInCreationOrder()
    {
        var first = SequentialGuid.Create();
        Thread.Sleep(3);
        var second = SequentialGuid.Create();

        string.CompareOrdinal(first.ToString(), second.ToString()).Should().BeNegative();
    }

    [Fact]
    public void Create_ProducesDistinctValues()
    {
        var values = Enumerable.Range(0, 1000).Select(_ => SequentialGuid.Create()).ToList();

        values.Distinct().Should().HaveCount(1000);
    }
}
