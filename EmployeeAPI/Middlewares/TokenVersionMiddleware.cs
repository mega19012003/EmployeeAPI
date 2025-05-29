using EmployeeAPI.Repositories.Users;
using System.Security.Claims;

namespace EmployeeAPI.Middlewares
{
    public class TokenVersionMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenVersionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserRepository userRepository)
        {
            // Bỏ qua middleware nếu là endpoint login (hoặc các endpoint khác nếu cần)
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (
                path.Contains("/login") ||
                path.Contains("/register") ||
                path.Contains("/refresh-token")
            ))
            {
                await _next(context);
                return;
            }
            var user = context.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenVersionClaim = user.FindFirst("TokenVersion")?.Value;

                if (!string.IsNullOrEmpty(userIdClaim) && !string.IsNullOrEmpty(tokenVersionClaim))
                {
                    var userEntity = await userRepository.GetByIdAsync(Guid.Parse(userIdClaim));
                    if (userEntity != null)
                    {
                        var dbTokenVersion = userEntity.TokenVersion.ToString();
                        if (tokenVersionClaim != dbTokenVersion)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                Message = "Token is no longer valid",
                                Detail = "Your token version does not match the current version in the database. Please log in again.",
                                StatusCode = 401
                            });
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
