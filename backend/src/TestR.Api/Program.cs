using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TestR.Api.Endpoints;
using TestR.Api.Infrastructure;
using TestR.Application.Users;
using TestR.Infrastructure;
using TestR.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var auth = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()
           ?? new AuthOptions();

var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=app.db";

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddScoped<IListUsers, ListUsersHandler>();
builder.Services.AddScoped<IGetUser, GetUserHandler>();
builder.Services.AddScoped<ICreateUser, CreateUserHandler>();
builder.Services.AddScoped<IUpdateUser, UpdateUserHandler>();
builder.Services.AddScoped<IDeleteUser, DeleteUserHandler>();

builder.Services.AddApiAuth(auth);
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{

    options.SupportNonNullableReferenceTypes();
    options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
    options.UseAllOfToExtendReferenceSchemas();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TestR User Directory API",
        Version = "v1",
        Description = "CRUD over the user directory. Reads are public; writes require a bearer token when auth is enabled.",
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste an OIDC access token (without the \"Bearer \" prefix).",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme,
            },
        }] = [],
    });
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));
}

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    EnsureDatabaseDirectoryExists(connectionString);
    await db.Database.MigrateAsync();
}

app.UseExceptionHandling();

if (corsOrigins.Length > 0)
{
    app.UseCors();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TestR User Directory API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapUsers();
app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" }))
   .WithTags("Health")
   .ExcludeFromDescription();

app.Run();

static void EnsureDatabaseDirectoryExists(string connectionString)
{
    var dataSource = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString).DataSource;
    if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
    {
        return;
    }

    var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }
}

public partial class Program;
