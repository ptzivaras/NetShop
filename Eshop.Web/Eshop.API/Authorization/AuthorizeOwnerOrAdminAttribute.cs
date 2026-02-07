using Microsoft.AspNetCore.Authorization;

namespace Eshop.API.Authorization
{
    public class AuthorizeOwnerOrAdminAttribute : AuthorizeAttribute
    {
        public AuthorizeOwnerOrAdminAttribute(string userIdParameter = "userId")
        {
            Policy = $"OwnerOrAdmin_{userIdParameter}";
        }
    }
}
