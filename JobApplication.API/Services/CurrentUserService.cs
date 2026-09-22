using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using System.Globalization;
using System.Security.Claims;

namespace JobApplication.API.Services
{
    /// <summary>
    /// Implements ICurrentUserService by reading the claims of the current
    /// request's HttpContext.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var claimValue = _httpContextAccessor.HttpContext?.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                return int.TryParse(claimValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                    ? id
                    : null;
            }
        }

        public string? Role
            => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

        public int GetRequiredUserId()
            => UserId ?? throw new UnauthorizedException("The token does not contain a valid user identity.");
    }
}
