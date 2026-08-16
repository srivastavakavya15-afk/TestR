using TestR.Application.Abstractions;
using TestR.Domain.Users;

namespace TestR.Application.Users;

public sealed class ListUsersHandler(IUserRepository users) : IListUsers
{
    public Task<IReadOnlyList<UserDto>> HandleAsync(CancellationToken ct) => users.ListAsync(ct);
}

public sealed class GetUserHandler(IUserRepository users) : IGetUser
{
    public Task<UserDto?> HandleAsync(Guid id, CancellationToken ct) => users.GetDtoByIdAsync(id, ct);
}

public sealed class CreateUserHandler(IUserRepository users) : ICreateUser
{
    public async Task<UserDto> HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        var user = new User(request.Name, request.Age, request.City, request.State, request.Pincode);

        await users.AddAsync(user, ct);
        await users.SaveChangesAsync(ct);

        return user.ToDto();
    }
}

public sealed class UpdateUserHandler(IUserRepository users) : IUpdateUser
{
    public async Task<UserDto?> HandleAsync(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct);
        if (user is null)
        {
            return null;
        }

        user.Update(request.Name, request.Age, request.City, request.State, request.Pincode);
        await users.SaveChangesAsync(ct);

        return user.ToDto();
    }
}

public sealed class DeleteUserHandler(IUserRepository users) : IDeleteUser
{
    public async Task<bool> HandleAsync(Guid id, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(id, ct);
        if (user is null)
        {
            return false;
        }

        users.Remove(user);
        await users.SaveChangesAsync(ct);

        return true;
    }
}
