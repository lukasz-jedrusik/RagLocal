using Microsoft.AspNetCore.Http;

namespace Rag.Services.Backend.Application.Interfaces.Services
{
    public interface IIdentityService
    {
        int GetUserId(HttpContext context);
    }
}
