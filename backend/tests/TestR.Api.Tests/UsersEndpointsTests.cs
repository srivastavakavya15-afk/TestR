using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using TestR.Application.Users;

namespace TestR.Api.Tests;

public sealed class UsersEndpointsTests : IClassFixture<ApiFactory>, IDisposable
{
    private static readonly JsonSerializerOptions Json =
        new(JsonSerializerDefaults.Web);

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public UsersEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateApiClient();
    }

    public void Dispose() => _client.Dispose();

    private static CreateUserRequest Ada() =>
        new("Ada Lovelace", 36, "London", "Greater London", "WC1E");

    private async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/users", request, Json);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<UserDto>(Json);
        created.Should().NotBeNull();
        return created!;
    }

    [Fact]
    public async Task GetAll_WithNoUsers_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>(Json);
        users.Should().BeEmpty();
    }

    [Fact]
    public async Task Post_WithValidBody_Returns201WithLocationAndBody()
    {
        var response = await _client.PostAsJsonAsync("/api/users", Ada(), Json);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<UserDto>(Json);
        created.Should().NotBeNull();
        created!.Id.Should().NotBe(Guid.Empty);
        created.Name.Should().Be("Ada Lovelace");
        created.Age.Should().Be(36);
        created.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/users/{created.Id}");
    }

    [Fact]
    public async Task Post_WithInvalidBody_Returns400WithPerFieldErrors()
    {
        var invalid = new CreateUserRequest("A", 999, "", "", "1");

        var response = await _client.PostAsJsonAsync("/api/users", invalid, Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = document.RootElement.GetProperty("errors");

        foreach (var field in new[] { "name", "age", "city", "state", "pincode" })
        {
            errors.TryGetProperty(field, out var messages).Should().BeTrue($"'{field}' should be reported");
            messages.EnumerateArray().Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task Post_WithMalformedJson_Returns400()
    {
        using var content = new StringContent("{ not json", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/users", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_TrimsWhitespaceBeforePersisting()
    {
        var created = await CreateAsync(new CreateUserRequest("  Ada Lovelace  ", 36, " London ", " Greater London ", " WC1E "));

        created.Name.Should().Be("Ada Lovelace");
        created.City.Should().Be("London");
        created.State.Should().Be("Greater London");
        created.Pincode.Should().Be("WC1E");
    }

    [Fact]
    public async Task GetById_WithKnownId_Returns200()
    {
        var created = await CreateAsync(Ada());

        var response = await _client.GetAsync($"/api/users/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<UserDto>(Json);
        fetched!.Id.Should().Be(created.Id);
        fetched.Name.Should().Be("Ada Lovelace");
    }

    [Fact]
    public async Task GetById_WithUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithNonGuidId_Returns404FromTheRouteConstraint()
    {
        var response = await _client.GetAsync("/api/users/not-a-guid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ReturnsNewestFirst()
    {
        await CreateAsync(Ada());
        await CreateAsync(new CreateUserRequest("Grace Hopper", 45, "Arlington", "VA", "22201"));

        var users = await _client.GetFromJsonAsync<List<UserDto>>("/api/users", Json);

        users!.Select(u => u.Name).Should().Equal("Grace Hopper", "Ada Lovelace");
    }

    [Fact]
    public async Task Put_WithKnownId_Returns200AndPersistsTheChange()
    {
        var created = await CreateAsync(Ada());
        var update = new UpdateUserRequest("Grace Hopper", 45, "Arlington", "VA", "22201");

        var response = await _client.PutAsJsonAsync($"/api/users/{created.Id}", update, Json);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<UserDto>(Json);
        updated!.Name.Should().Be("Grace Hopper");
        updated.Id.Should().Be(created.Id);
        updated.CreatedAtUtc.Should().Be(created.CreatedAtUtc);

        var refetched = await _client.GetFromJsonAsync<UserDto>($"/api/users/{created.Id}", Json);
        refetched!.Name.Should().Be("Grace Hopper");
        refetched.Pincode.Should().Be("22201");
    }

    [Fact]
    public async Task Put_WithUnknownId_Returns404()
    {
        var update = new UpdateUserRequest("Grace Hopper", 45, "Arlington", "VA", "22201");

        var response = await _client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", update, Json);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_WithInvalidBody_Returns400BeforeTouchingTheDatabase()
    {
        var created = await CreateAsync(Ada());

        var response = await _client.PutAsJsonAsync(
            $"/api/users/{created.Id}",
            new UpdateUserRequest("A", 999, "", "", "1"),
            Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var unchanged = await _client.GetFromJsonAsync<UserDto>($"/api/users/{created.Id}", Json);
        unchanged!.Name.Should().Be("Ada Lovelace");
    }

    [Fact]
    public async Task Delete_WithKnownId_Returns204AndRemovesTheUser()
    {
        var created = await CreateAsync(Ada());

        var response = await _client.DeleteAsync($"/api/users/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.GetAsync($"/api/users/{created.Id}")).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithUnknownId_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Twice_Returns404TheSecondTime()
    {
        var created = await CreateAsync(Ada());

        (await _client.DeleteAsync($"/api/users/{created.Id}")).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
        (await _client.DeleteAsync($"/api/users/{created.Id}")).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Swagger_DocumentIsServedInDevelopment()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("paths").TryGetProperty("/api/users", out _)
            .Should().BeTrue();
    }

    [Fact]
    public async Task Post_WhenAuthIsDisabled_DoesNotRequireAToken()
    {

        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync("/api/users", Ada(), Json);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
