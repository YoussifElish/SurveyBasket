
using System.Security.Claims;

namespace SurveyBasket.Authentication.Filters
{
    public class PermissionAuthorizationHandler(RoleManager<ApplicationRole> roleManager) : AuthorizationHandler<PermissionRequirment>
    {
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirment requirement)
        {
            var user = context.User.Identity;

            //if (user is null || !user.IsAuthenticated)
            //    return;

            //var hasPermission = context.User.Claims.Any(x => x.Value == requirement.Permission && x.Type == Permissions.Type);

            //if (!hasPermission)
            //    return;
            var userRole = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            var isRoleDeleted = await _roleManager.Roles.AnyAsync(r => r.Name == userRole && r.IsDeleted);

            if (context.User.Identity is not { IsAuthenticated: true } ||
                 !context.User.Claims.Any(x => x.Value == requirement.Permission && x.Type == Permissions.Type))
                return;



            context.Succeed(requirement);
            return;

        }
    }
}
