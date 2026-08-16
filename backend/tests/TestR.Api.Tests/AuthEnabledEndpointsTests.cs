using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using TestR.Application.Users;

namespace TestR.Api.Tests;

public sealed class AuthEnabledApiFactory : ApiFactoryBase
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("Auth:Enabled", "true");
        builder.UseSetting("Auth:Authority", "https://login.microsoftonline.com/common/v2.0");
        builder.UseSetting("Auth:Audience", "api://testr");
    }
}

public sealed class AuthEnabledEndpointsTests : IClassFixture<AuthEnabledApiFactory>, IDisposable
{
    private readonly HttpClient _client;

    public AuthEnabledEndpointsTests(AuthEnabledApiFactory factory) =>
        _client = factory.CreateApiClient();

    public void Dispose() => _client.Dispose();

    private static CreateUserRequest Ada() =>
        new("Ada Lovelace", 36, "London", "Greater London", "WC1E");

    [Fact]
    public async Task GetAll_WithoutAToken_IsStillPublic()
    {
        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WithoutAToken_IsStillPublic()
    {

        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_WithoutAToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/users", Ada());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Put_WithoutAToken_Returns401()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/users/{Guid.NewGuid()}",
            new UpdateUserRequest("Grace Hopper", 45, "Arlington", "VA", "22201"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_WithoutAToken_Returns401()
    {
        var response = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WithAGarbageToken_Returns401RatherThanAcceptingIt()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "not-a-real-jwt");

        var response = await _client.PostAsJsonAsync("/api/users", Ada());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
