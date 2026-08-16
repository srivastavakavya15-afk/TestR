namespace TestR.Application.Users;

public interface IUserWriteRequest
{
    string Name { get; }
    int Age { get; }
    string City { get; }
    string State { get; }
    string Pincode { get; }
}

public sealed record CreateUserRequest(string Name, int Age, string City, string State, string Pincode)
    : IUserWriteRequest;

public sealed record UpdateUserRequest(string Name, int Age, string City, string State, string Pincode)
    : IUserWriteRequest;

public sealed record UserDto(
    Guid Id,
    string Name,
    int Age,
    string City,
    string State,
    string Pincode,
    DateTime CreatedAtUtc);
