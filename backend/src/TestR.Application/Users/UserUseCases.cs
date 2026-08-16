namespace TestR.Application.Users;

public interface IListUsers
{
    Task<IReadOnlyList<UserDto>> HandleAsync(CancellationToken ct);
}

public interface IGetUser
{
    Task<UserDto?> HandleAsync(Guid id, CancellationToken ct);
}

public interface ICreateUser
{
    Task<UserDto> HandleAsync(CreateUserRequest request, CancellationToken ct);
}

public interface IUpdateUser
{

    Task<UserDto?> HandleAsync(Guid id, UpdateUserRequest request, CancellationToken ct);
}

public interface IDeleteUser
{

    Task<bool> HandleAsync(Guid id, CancellationToken ct);
}
