using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Organizations;
using Abp.UI;
using Pensees.Charon.Authorization.Roles;
using Pensees.Charon.Authorization.Users;
using Pensees.Charon.Organizations.Dto;

namespace Pensees.Charon.Organizations
{
    public class OrganizationUnitAppService : AsyncCrudAppService<OrganizationUnit, OrganizationUnitDto, long,
        PagedResultRequestDto, CreateOrganizationUnitDto, OrganizationUnitDto>, IOrganizationUnitAppService
    {
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly OrganizationUnitManager _organizationUnitManager;

        private const string TenantMismatchError = "The specified tenant Id is inconsistent with the current tenant Id.";

        public OrganizationUnitAppService(
            IRepository<OrganizationUnit, long> orgUnitRepository,
            RoleManager roleManager,
            UserManager userManager,
            OrganizationUnitManager organizationUnitManager)
            : base(orgUnitRepository)
        {
            _orgUnitRepository = orgUnitRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _organizationUnitManager = organizationUnitManager;
        }

        public override Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitDto input)
        {
            if (input.TenantId != GetCurrentTenantId())
            {
                throw new UserFriendlyException(TenantMismatchError);
            }

            if (string.IsNullOrEmpty(input.Code))
            {
                input.Code = _organizationUnitManager.GetNextChildCode(input.ParentId);
            }

            return base.CreateAsync(input);
        }

        private int? GetCurrentTenantId()
        {
            if (CurrentUnitOfWork != null)
            {
                return CurrentUnitOfWork.GetTenantId();
            }

            return AbpSession.TenantId;
        }

        public async Task AddRoleToOrganizationUnitAsync(SetOrganizationUnitRoleDto input)
        { 
            await CheckExistenceOfRoleAndOuAsync(input);

            await _roleManager.AddToOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId, _userManager.AbpSession.TenantId);
        }

        private async Task CheckExistenceOfRoleAndOuAsync(SetOrganizationUnitRoleDto input)
        {
            try
            {
                var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                var ou = await _orgUnitRepository.GetAsync(input.OrganizationUnitId);
            }
            catch (Exception exception)
            {
                throw new UserFriendlyException(exception.Message);
            }
        }

        public async Task RemoveRoleFromOrganizationUnitAsync(SetOrganizationUnitRoleDto input)
        {
            await CheckExistenceOfRoleAndOuAsync(input);

            await _roleManager.RemoveFromOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId);
        }

        public async Task AddUserToOrganizationAsync(SetOrganizationUnitUserDto input)
        {
            await CheckExistenceOfUserAndOuAsync(input);

            await _userManager.AddToOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
        }

        private async Task CheckExistenceOfUserAndOuAsync(SetOrganizationUnitUserDto input)
        {
            try
            {
                var user = await _userManager.GetUserByIdAsync(input.UserId);
                var ou = await _orgUnitRepository.GetAsync(input.OrganizationUnitId);
            }
            catch (Exception exception)
            {
                throw new UserFriendlyException(exception.Message);
            }
        }

        public async Task RemoveUserToOrganizationAsync(SetOrganizationUnitUserDto input)
        {
            await CheckExistenceOfUserAndOuAsync(input);

            await _userManager.RemoveFromOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
        }
    }
}
