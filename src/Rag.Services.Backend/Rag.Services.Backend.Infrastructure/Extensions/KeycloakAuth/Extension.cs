using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Rag.Services.Backend.Infrastructure.Extensions.KeycloakAuth
{
    public static class Extension
    {
        public static IServiceCollection AddKeycloakAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    o =>
                    {
                        o.RequireHttpsMetadata = false;
                        o.Authority = configuration["Keycloak:AuthorityUrl"];
                        o.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateAudience = false,
                            ValidAudience = configuration["Keycloak:ClientId"],
                            ValidateIssuerSigningKey = true,
                            ValidateIssuer = true,
                            ValidIssuer = configuration["Keycloak:AuthorityUrl"],
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                        };
                    });

            services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .Build());

            return services;
        }
    }
}