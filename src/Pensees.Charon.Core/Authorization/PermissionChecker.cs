using Abp.Authorization;
using Pensees.Charon.Authorization.Roles;
using Pensees.Charon.Authorization.Users;

namespace Pensees.Charon.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
