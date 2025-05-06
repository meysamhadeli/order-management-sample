using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrderManagement.Identities.Constants;

namespace BuildingBlocks.Web
{
    public interface ICurrentUserProvider
    {
        string? GetCurrentUserId();
        string[] GetCurrentUserRoles();
        bool IsAuthenticated();
        bool IsAdmin();
    }

    public class CurrentUserProvider : ICurrentUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentUserId()
        {
            var nameIdentifier = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return nameIdentifier;
        }

        public string[] GetCurrentUserRoles()
        {
            return _httpContextAccessor?.HttpContext?.User?
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray() ?? [];
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public bool IsAdmin()
        {
            return _httpContextAccessor?.HttpContext?.User?
                .IsInRole(IdentityConstant.Role.Admin) ?? false;
        }
    }
}
