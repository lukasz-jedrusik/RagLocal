using Microsoft.AspNetCore.Http;
using Rag.Services.Backend.Application.Interfaces.Services;

namespace Rag.Services.Backend.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        public int GetUserId(HttpContext context)
        {
            var userIdClaim = context.User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }
    }
}
