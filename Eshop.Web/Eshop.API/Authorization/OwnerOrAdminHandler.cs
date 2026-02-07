using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Eshop.API.Authorization
{
    public class OwnerOrAdminHandler : AuthorizationHandler<OwnerOrAdminRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OwnerOrAdminHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OwnerOrAdminRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Task.CompletedTask;
            }

            // Check if user is Admin
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Get current user ID
            var currentUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Task.CompletedTask;
            }

            // Get requested user ID from route/query
            var routeValues = httpContext.Request.RouteValues;
            var queryParams = httpContext.Request.Query;

            string? requestedUserId = null;

            // Try route values first
            if (routeValues.TryGetValue(requirement.UserIdParameterName, out var routeValue))
            {
                requestedUserId = routeValue?.ToString();
            }
            // Then try query parameters
            else if (queryParams.TryGetValue(requirement.UserIdParameterName, out var queryValue))
            {
                requestedUserId = queryValue.ToString();
            }
            // Finally try request body (from DTO)
            else
            {
                // For POST requests with body, we'll handle validation in controller
                // since we can't easily access deserialized body here
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check if user owns the resource
            if (requestedUserId == currentUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
