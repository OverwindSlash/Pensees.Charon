using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Pensees.Charon.Authorization.Roles;
using Pensees.Charon.Authorization.Users;
using Pensees.Charon.Sessions.Dto;

namespace Pensees.Charon.Sessions
{
    public class SessionAppService : CharonAppServiceBase, ISessionAppService
    {
        private readonly IRepository<User, long> _repository;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;

        public SessionAppService(
            IRepository<User, long> repository,
            RoleManager roleManager,
            UserManager userManager)
        {
            _repository = repository;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [DisableAuditing]
        public async Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations()
        {
            var output = new GetCurrentLoginInformationsOutput
            {
                Application = new ApplicationInfoDto
                {
                    Version = AppVersionHelper.Version,
                    ReleaseDate = AppVersionHelper.ReleaseDate,
                    Features = new Dictionary<string, bool>()
                }
            };

            if (AbpSession.TenantId.HasValue)
            {
                output.Tenant = ObjectMapper.Map<TenantLoginInfoDto>(await GetCurrentTenantAsync());
            }

            if (AbpSession.UserId.HasValue)
            {
                long uid = AbpSession.GetUserId();

                var user = await _repository.GetAllIncluding(
                    x => x.Roles).FirstOrDefaultAsync(x => x.Id == uid);

                output.User = ObjectMapper.Map<UserLoginInfoDto>(user);

                // Roles
                if (user.Roles != null)
                {
                    var roleIds = user.Roles.Select(x => x.RoleId).ToArray();

                    var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

                    output.User.Roles = roles.ToArray();


                    // Permissions
                    List<Permission> permissions = new List<Permission>();

                    foreach (UserRole role in user.Roles)
                    {
                        permissions.AddRange(await _roleManager.GetGrantedPermissionsAsync(role.RoleId));
                    }

                    output.User.Permissions = permissions.Distinct().Select(p => p.Name).ToArray();
                }

                // Organization Units
                var ous = _userManager.GetOrganizationUnits(user).Select(ou => ou.DisplayName);
                output.User.OrgUnits = ous.ToArray();
            }

            return output;
        }
    }
}
