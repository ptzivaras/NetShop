using Microsoft.AspNetCore.Authorization;

namespace Eshop.API.Authorization
{
    public class OwnerOrAdminRequirement : IAuthorizationRequirement
    {
        public string UserIdParameterName { get; }

        public OwnerOrAdminRequirement(string userIdParameterName = "userId")
        {
            UserIdParameterName = userIdParameterName;
        }
    }
}
