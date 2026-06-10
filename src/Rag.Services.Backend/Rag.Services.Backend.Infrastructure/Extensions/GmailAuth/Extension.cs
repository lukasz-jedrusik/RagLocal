using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Rag.Services.Backend.Domain.Models;
using Rag.Services.Backend.Infrastructure.Extensions.EfCore;

namespace Rag.Services.Backend.Infrastructure.Extensions.GmailAuth
{
    public static class Extension
    {
        public static IServiceCollection AddGmailAuthorization(this IServiceCollection services, IConfiguration configuration)
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
                        o.RequireHttpsMetadata = true;
                        o.Authority = "https://accounts.google.com";
                        o.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateAudience = true,
                            ValidAudience = configuration["Google:ClientId"],
                            ValidateIssuer = true,
                            ValidIssuers = new[] { "https://accounts.google.com", "accounts.google.com" },
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                        };

                        // Event handler to create user in database on first login
                        o.Events = new JwtBearerEvents
                        {
                            OnTokenValidated = async context =>
                            {
                                var dbContext = context.HttpContext.RequestServices.GetRequiredService<DataContext>();

                                // Get user info from token claims
                                var googleId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
                                var name = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
                                var pictureUrl = context.Principal?.FindFirst("picture")?.Value;

                                if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
                                {
                                    context.Fail("Invalid token claims");
                                    return;
                                }

                                // Find or create user
                                var user = await dbContext.Users
                                    .FirstOrDefaultAsync(u => u.GoogleId == googleId);

                                if (user == null)
                                {
                                    // Create new user
                                    user = new User
                                    {
                                        GoogleId = googleId,
                                        Email = email,
                                        Name = name ?? email,
                                        PictureUrl = pictureUrl,
                                        CreatedAt = DateTime.UtcNow,
                                        LastLoginAt = DateTime.UtcNow
                                    };

                                    dbContext.Users.Add(user);
                                    await dbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    // Update last login
                                    user.LastLoginAt = DateTime.UtcNow;
                                    user.Name = name ?? user.Name;
                                    user.PictureUrl = pictureUrl ?? user.PictureUrl;
                                    await dbContext.SaveChangesAsync();
                                }

                                // Add user ID to claims for easy access
                                var claims = new List<Claim>
                                {
                                    new Claim("user_id", user.Id.ToString())
                                };

                                var appIdentity = new ClaimsIdentity(claims);
                                context.Principal?.AddIdentity(appIdentity);
                            }
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
