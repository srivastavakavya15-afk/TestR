using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace TestR.Api.Infrastructure;

public static class AuthPolicies
{

    public const string WriteAccess = "WriteAccess";

    public static IServiceCollection AddApiAuth(this IServiceCollection services, AuthOptions auth)
    {
        auth.ValidateOnStart();

        if (auth.IsConfigured)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = auth.Authority;
                    options.RequireHttpsMetadata = auth.RequireHttpsMetadata;
                    options.TokenValidationParameters.ValidateAudience =
                        !string.IsNullOrWhiteSpace(auth.Audience);

                    if (!string.IsNullOrWhiteSpace(auth.Audience))
                    {
                        options.Audience = auth.Audience;
                    }
                });
        }
        else
        {

            services.AddAuthentication(AnonymousAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, AnonymousAuthenticationHandler>(
                    AnonymousAuthenticationHandler.SchemeName, _ => { });
        }

        services.AddAuthorizationBuilder()
            .AddPolicy(WriteAccess, policy =>
            {
                if (auth.IsConfigured)
                {
                    policy.RequireAuthenticatedUser();
                }
                else
                {

                    policy.RequireAssertion(_ => true);
                }
            });

        return services;
    }
}
