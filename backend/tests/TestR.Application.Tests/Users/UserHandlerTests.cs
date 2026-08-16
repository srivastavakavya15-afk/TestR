using FluentAssertions;
using TestR.Application.Users;
using TestR.Domain;
using TestR.Domain.Users;

namespace TestR.Application.Tests.Users;

public class CreateUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidRequest_PersistsAndReturnsTheDto()
    {
        var repository = new FakeUserRepository();
        var handler = new CreateUserHandler(repository);

        var dto = await handler.HandleAsync(
            new CreateUserRequest("Ada Lovelace", 36, "London", "Greater London", "WC1E"),
            CancellationToken.None);

        dto.Id.Should().NotBe(Guid.Empty);
        dto.Name.Should().Be("Ada Lovelace");
        repository.SaveChangesCallCount.Should().Be(1);

        var stored = await repository.ListAsync(CancellationToken.None);
        stored.Should().ContainSingle().Which.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task HandleAsync_WithValuesTheDomainRejects_ThrowsAndSavesNothing()
    {
        var repository = new FakeUserRepository();
        var handler = new CreateUserHandler(repository);

        var act = () => handler.HandleAsync(
            new CreateUserRequest("A", 36, "London", "Greater London", "WC1E"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        repository.SaveChangesCallCount.Should().Be(0);
    }
}

public class ListUsersHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithNoUsers_ReturnsEmptyList()
    {
        var handler = new ListUsersHandler(new FakeUserRepository());

        var users = await handler.HandleAsync(CancellationToken.None);

        users.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsNewestFirst()
    {
        var repository = new FakeUserRepository();
        var older = new User("Ada Lovelace", 36, "London", "Greater London", "WC1E");
        await Task.Delay(5);
        var newer = new User("Grace Hopper", 45, "Arlington", "VA", "22201");
        repository.Seed(older, newer);

        var users = await new ListUsersHandler(repository)
            .HandleAsync(CancellationToken.None);

        users.Select(u => u.Name).Should().Equal("Grace Hopper", "Ada Lovelace");
    }
}

public class GetUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithUnknownId_ReturnsNull()
    {
        var handler = new GetUserHandler(new FakeUserRepository());

        var user = await handler.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        user.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithKnownId_ReturnsTheUser()
    {
        var repository = new FakeUserRepository();
        var existing = new User("Ada Lovelace", 36, "London", "Greater London", "WC1E");
        repository.Seed(existing);

        var user = await new GetUserHandler(repository)
            .HandleAsync(existing.Id, CancellationToken.None);

        user.Should().NotBeNull();
        user!.Name.Should().Be("Ada Lovelace");
    }
}

public class UpdateUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithUnknownId_ReturnsNullAndSavesNothing()
    {
        var repository = new FakeUserRepository();

        var result = await new UpdateUserHandler(repository).HandleAsync(
            Guid.NewGuid(),
            new UpdateUserRequest("Grace Hopper", 45, "Arlington", "VA", "22201"),
            CancellationToken.None);

        result.Should().BeNull();
        repository.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithKnownId_UpdatesAndReturnsTheDto()
    {
        var repository = new FakeUserRepository();
        var existing = new User("Ada Lovelace", 36, "London", "Greater London", "WC1E");
        repository.Seed(existing);

        var result = await new UpdateUserHandler(repository).HandleAsync(
            existing.Id,
            new UpdateUserRequest("Grace Hopper", 45, "Arlington", "VA", "22201"),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Grace Hopper");
        result.Id.Should().Be(existing.Id);
        repository.SaveChangesCallCount.Should().Be(1);
    }
}

public class DeleteUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithUnknownId_ReturnsFalse()
    {
        var repository = new FakeUserRepository();

        var deleted = await new DeleteUserHandler(repository)
            .HandleAsync(Guid.NewGuid(), CancellationToken.None);

        deleted.Should().BeFalse();
        repository.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithKnownId_RemovesTheUser()
    {
        var repository = new FakeUserRepository();
        var existing = new User("Ada Lovelace", 36, "London", "Greater London", "WC1E");
        repository.Seed(existing);

        var deleted = await new DeleteUserHandler(repository)
            .HandleAsync(existing.Id, CancellationToken.None);

        deleted.Should().BeTrue();
        (await repository.ListAsync(CancellationToken.None)).Should().BeEmpty();
    }
}
