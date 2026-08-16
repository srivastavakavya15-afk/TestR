using TestR.Application.Users;
using TestR.Domain.Users;

namespace TestR.Application.Abstractions;

public interface IUserRepository
{

    Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct);

    Task<UserDto?> GetDtoByIdAsync(Guid id, CancellationToken ct);

    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);

    Task AddAsync(User user, CancellationToken ct);

    void Remove(User user);

    Task SaveChangesAsync(CancellationToken ct);
}
