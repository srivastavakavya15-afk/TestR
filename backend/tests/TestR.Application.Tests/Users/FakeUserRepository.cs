using TestR.Application.Abstractions;
using TestR.Application.Users;
using TestR.Domain.Users;

namespace TestR.Application.Tests.Users;

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    public int SaveChangesCallCount { get; private set; }

    public void Seed(params User[] users) => _users.AddRange(users);

    public Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<UserDto>>(
            _users.OrderByDescending(u => u.CreatedAtUtc).Select(ToDto).ToList());

    public Task<UserDto?> GetDtoByIdAsync(Guid id, CancellationToken ct)
    {
        var user = _users.SingleOrDefault(u => u.Id == id);
        return Task.FromResult(user is null ? null : ToDto(user));
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_users.SingleOrDefault(u => u.Id == id));

    public Task AddAsync(User user, CancellationToken ct)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public void Remove(User user) => _users.Remove(user);

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.Name, u.Age, u.City, u.State, u.Pincode, u.CreatedAtUtc);
}
