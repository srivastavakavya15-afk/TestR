using TestR.Domain.Users;

namespace TestR.Application.Users;

internal static class UserMappings
{
    public static UserDto ToDto(this User user) => new(
        user.Id,
        user.Name,
        user.Age,
        user.City,
        user.State,
        user.Pincode,
        user.CreatedAtUtc);
}
