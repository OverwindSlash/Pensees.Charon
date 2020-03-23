using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Organizations;
using Pensees.Charon.Authorization.Roles;
using Pensees.Charon.Authorization.Users;
using Pensees.Charon.Organizations.Dto;

namespace Pensees.Charon.Organizations
{
    public class OrganizationUnitAppService : AsyncCrudAppService<OrganizationUnit, OrganizationUnitDto, long,
        PagedResultRequestDto, OrganizationUnitDto, OrganizationUnitDto>, IOrganizationUnitAppService
    {
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;

        public OrganizationUnitAppService(
            IRepository<OrganizationUnit, long> orgUnitRepository,
            RoleManager roleManager,
            UserManager userManager)
            : base(orgUnitRepository)
        {
            _orgUnitRepository = orgUnitRepository;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task AddRoleToOrganizationUnitAsync(SetOrganizationUnitRoleDto input)
        {
            await _roleManager.AddToOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId, _userManager.AbpSession.TenantId);
        }

        public async Task RemoveRoleFromOrganizationUnitAsync(SetOrganizationUnitRoleDto input)
        {
            await _roleManager.RemoveFromOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId);
        }

        public async Task AddUserToOrganizationAsync(SetOrganizationUnitUserDto input)
        {
            await _userManager.AddToOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
        }

        public async Task RemoveUserToOrganizationAsync(SetOrganizationUnitUserDto input)
        {
            await _userManager.RemoveFromOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
        }
    }
}
