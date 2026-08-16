using Microsoft.EntityFrameworkCore;
using TestR.Application.Abstractions;
using TestR.Application.Users;
using TestR.Domain.Users;

namespace TestR.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct) =>
        await db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new UserDto(u.Id, u.Name, u.Age, u.City, u.State, u.Pincode, u.CreatedAtUtc))
            .ToListAsync(ct);

    public Task<UserDto?> GetDtoByIdAsync(Guid id, CancellationToken ct) =>
        db.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDto(u.Id, u.Name, u.Age, u.City, u.State, u.Pincode, u.CreatedAtUtc))
            .SingleOrDefaultAsync(ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.SingleOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct) =>
        await db.Users.AddAsync(user, ct);

    public void Remove(User user) => db.Users.Remove(user);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
