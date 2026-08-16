using Microsoft.AspNetCore.Http.HttpResults;
using TestR.Api.Infrastructure;
using TestR.Application.Users;

namespace TestR.Api.Endpoints;

public static class UsersEndpoints
{

    public static IEndpointRouteBuilder MapUsers(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", GetAll)
             .WithName("ListUsers")
             .WithSummary("List all users, newest first.");

        group.MapGet("/{id:guid}", GetById)
             .WithName("GetUser")
             .WithSummary("Get a single user by id.");

        group.MapPost("/", Create)
             .WithName("CreateUser")
             .WithSummary("Create a user.")
             .WithValidation<CreateUserRequest>()
             .RequireAuthorization(AuthPolicies.WriteAccess);

        group.MapPut("/{id:guid}", Update)
             .WithName("UpdateUser")
             .WithSummary("Replace an existing user.")
             .WithValidation<UpdateUserRequest>()
             .RequireAuthorization(AuthPolicies.WriteAccess);

        group.MapDelete("/{id:guid}", Delete)
             .WithName("DeleteUser")
             .WithSummary("Delete a user.")
             .RequireAuthorization(AuthPolicies.WriteAccess);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<UserDto>>> GetAll(
        IListUsers handler, CancellationToken ct) =>
        TypedResults.Ok(await handler.HandleAsync(ct));

    private static async Task<Results<Ok<UserDto>, NotFound>> GetById(
        Guid id, IGetUser handler, CancellationToken ct)
    {
        var user = await handler.HandleAsync(id, ct);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
    }

    private static async Task<Created<UserDto>> Create(
        CreateUserRequest request, ICreateUser handler, CancellationToken ct)
    {
        var user = await handler.HandleAsync(request, ct);
        return TypedResults.Created($"/api/users/{user.Id}", user);
    }

    private static async Task<Results<Ok<UserDto>, NotFound>> Update(
        Guid id, UpdateUserRequest request, IUpdateUser handler, CancellationToken ct)
    {
        var user = await handler.HandleAsync(id, request, ct);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
    }

    private static async Task<Results<NoContent, NotFound>> Delete(
        Guid id, IDeleteUser handler, CancellationToken ct)
    {
        var deleted = await handler.HandleAsync(id, ct);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
