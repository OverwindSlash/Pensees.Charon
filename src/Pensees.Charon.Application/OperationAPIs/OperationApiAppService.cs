using System;
using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Organizations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pensees.Charon.Authorization.Roles;
using Pensees.Charon.Authorization.Users;
using Pensees.Charon.MultiTenancy;
using Pensees.Charon.MultiTenancy.Dto;
using Pensees.Charon.OperationAPIs.Dto;
using Pensees.Charon.Organizations;
using Pensees.Charon.Organizations.Dto;
using Pensees.Charon.Roles.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.UI;

namespace Pensees.Charon.OperationAPIs
{
    public class OperationApiAppService : ApplicationService, IOperationApiAppService
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly TenantManager _tenantManager;
        private readonly IFeatureManager _featureManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IIocManager _iocManager;
        private readonly IAuthorizationConfiguration _authorizationConfiguration;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly OrganizationUnitManager _organizationUnitManager;

        public OperationApiAppService(
            ITenantAppService tenantAppService,
            TenantManager tenantManager,
            IFeatureManager featureManager,
            ILocalizationManager localizationManager,
            IIocManager iocManager,
            IAuthorizationConfiguration authorizationConfiguration,
            RoleManager roleManager,
            UserManager userManager,
            IRepository<OrganizationUnit, long> orgUnitRepository,
            OrganizationUnitManager organizationUnitManager)
        {
            _tenantAppService = tenantAppService;
            _tenantManager = tenantManager;
            _featureManager = featureManager;
            _localizationManager = localizationManager;
            _iocManager = iocManager;
            _authorizationConfiguration = authorizationConfiguration;
            _roleManager = roleManager;
            _userManager = userManager;
            _orgUnitRepository = orgUnitRepository;
            _organizationUnitManager = organizationUnitManager;
        }

        public Task<TenantDto> CreateTenantAsync(CreateTenantDto input)
        {
            return _tenantAppService.CreateAsync(input);
        }

        public Task<TenantDto> GetTenantAsync(EntityDto<int> input)
        {
            return _tenantAppService.GetAsync(input);
        }

        public Task<PagedResultDto<TenantDto>> GetAllTenantAsync(PagedTenantResultRequestDto input)
        {
            return _tenantAppService.GetAllAsync(input);
        }

        public Task<TenantDto> UpdateTenantAsync(TenantDto input)
        {
            return _tenantAppService.UpdateAsync(input);
        }

        public Task DeleteTenantAsync(EntityDto<int> input)
        {
            return _tenantAppService.DeleteAsync(input);
        }

        public Task<bool> ActivateTenantAsync(ActivateTenantDto input)
        {
            return _tenantAppService.ActivateTenant(input);
        }

        public List<FeatureDto> ListAllFeatures()
        {
            var features = _featureManager.GetAll();

            List<FeatureDto> featureDtos = new List<FeatureDto>();
            foreach (var feature in features)
            {
                featureDtos.Add(new FeatureDto()
                {
                    Name = feature.Name,
                    DisplayName = _localizationManager.GetString((LocalizableString)feature.DisplayName)
                });
            }

            return featureDtos;
        }

        public async Task<bool> EnableFeatureForTenantAsync(EnableFeatureDto input)
        {
            var tenant = await _tenantManager.GetByIdAsync(input.TenantId);

            if (tenant == null)
            {
                return false;
            }

            foreach (var featureName in input.FeatureNames)
            {
                var feature = _featureManager.GetOrNull(featureName);
                if (feature == null)
                {
                    continue;
                }

                await _tenantManager.SetFeatureValueAsync(tenant.Id, feature.Name, input.IsEnable.ToString());
            }

            return true;
        }


        public ListResultDto<PermissionDto> GetTenantPermissions(int tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                PermissionManager permissionManager = new PermissionManager(_iocManager, _authorizationConfiguration, UnitOfWorkManager);
                permissionManager.Initialize();
                var permissions = permissionManager.GetAllPermissions();

                return new ListResultDto<PermissionDto>(
                    ObjectMapper.Map<List<PermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList());
            }
        }

        public async Task<RoleDto> CreateRoleInTenantAsync(int tenantId, CreateRoleDto input)
        {
            var role = ObjectMapper.Map<Role>(input);
            role.SetNormalizedName();

            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                CheckErrors(await _roleManager.CreateAsync(role));

                var grantedPermissions = PermissionManager
                    .GetAllPermissions()
                    .Where(p => input.GrantedPermissions.Contains(p.Name))
                    .ToList();

                await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);
            }

            return ObjectMapper.Map<RoleDto>(role);
        }

        public async Task<ListResultDto<RoleListDto>> GetRolesInTenantAsync(int tenantId, GetRolesInput input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var roles = await _roleManager
                    .Roles
                    .WhereIf(
                        !input.Permission.IsNullOrWhiteSpace(),
                        r => r.Permissions.Any(rp => rp.Name == input.Permission && rp.IsGranted)
                    )
                    .ToListAsync();

                return new ListResultDto<RoleListDto>(ObjectMapper.Map<List<RoleListDto>>(roles));
            }
        }

        public async Task<RoleDto> UpdateRoleInTenantAsync(int tenantId, RoleDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var role = await _roleManager.GetRoleByIdAsync(input.Id);

                ObjectMapper.Map(input, role);

                CheckErrors(await _roleManager.UpdateAsync(role));

                var grantedPermissions = PermissionManager
                    .GetAllPermissions()
                    .Where(p => input.GrantedPermissions.Contains(p.Name))
                    .ToList();

                await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

                return ObjectMapper.Map<RoleDto>(role);
            }
        }

        public async Task DeleteRoleInTenantAsync(int tenantId, EntityDto<int> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var role = await _roleManager.FindByIdAsync(input.Id.ToString());
                var users = await _userManager.GetUsersInRoleAsync(role.NormalizedName);

                foreach (var user in users)
                {
                    CheckErrors(await _userManager.RemoveFromRoleAsync(user, role.NormalizedName));
                }

                CheckErrors(await _roleManager.DeleteAsync(role));
            }
        }

        public async Task<OrganizationUnitDto> CreateOuInTenantAsync(CreateOrganizationUnitDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                if (string.IsNullOrEmpty(input.Code))
                {
                    input.Code = _organizationUnitManager.GetNextChildCode(input.ParentId);
                }

                OrganizationUnit entity = ObjectMapper.Map<OrganizationUnit>(input);

                await _organizationUnitManager.CreateAsync(entity);

                return ObjectMapper.Map<OrganizationUnitDto>(entity);
            }
        }

        public async Task<OrganizationUnitDto> GetOuInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var entity = await _orgUnitRepository.GetAsync(input.Id);

                return ObjectMapper.Map<OrganizationUnitDto>(entity);
            }
        }

        public async Task<PagedResultDto<OrganizationUnitDto>> GetAllOuInTenantAsync(int tenantId, PagedResultRequestDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var query = _orgUnitRepository.GetAll();

                var totalCount = await query.CountAsync();

                query = ApplySorting(query, input);
                query = ApplyPaging(query, input);

                var entities = await query.ToListAsync();

                return new PagedResultDto<OrganizationUnitDto>(
                    totalCount,
                    entities.Select(MapToEntityDto).ToList()
                );
            }
        }

        public async Task<OrganizationUnitDto> UpdateOuInTenantAsync(OrganizationUnitDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                var entity = await _orgUnitRepository.GetAsync(input.Id);

                ObjectMapper.Map(input, entity);

                await CurrentUnitOfWork.SaveChangesAsync();

                return MapToEntityDto(entity);
            }
        }

        protected virtual OrganizationUnitDto MapToEntityDto(OrganizationUnit entity)
        {
            return ObjectMapper.Map<OrganizationUnitDto>(entity);
        }

        protected virtual IQueryable<OrganizationUnit> ApplySorting(IQueryable<OrganizationUnit> query, PagedResultRequestDto input)
        {
            //Try to sort query if available
            var sortInput = input as ISortedResultRequest;
            if (sortInput != null)
            {
                if (!sortInput.Sorting.IsNullOrWhiteSpace())
                {
                    return query.OrderBy(sortInput.Sorting);
                }
            }

            //IQueryable.Task requires sorting, so we should sort if Take will be used.
            if (input is ILimitedResultRequest)
            {
                return query.OrderByDescending(e => e.Id);
            }

            //No sorting
            return query;
        }

        protected virtual IQueryable<OrganizationUnit> ApplyPaging(IQueryable<OrganizationUnit> query, PagedResultRequestDto input)
        {
            //Try to use paging if available
            var pagedInput = input as IPagedResultRequest;
            if (pagedInput != null)
            {
                return query.PageBy(pagedInput);
            }

            //Try to limit query result if available
            var limitedInput = input as ILimitedResultRequest;
            if (limitedInput != null)
            {
                return query.Take(limitedInput.MaxResultCount);
            }

            //No paging
            return query;
        }

        public Task DeleteOuInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                return _orgUnitRepository.DeleteAsync(input.Id);
            }
        }

        public async Task AddRoleToOuInTenantAsync(int tenantId, SetOrganizationUnitRoleDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await CheckExistenceOfRoleAndOuAsync(input);

                await _roleManager.AddToOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId, _userManager.AbpSession.TenantId);
            }
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

        public async Task RemoveRoleFromOrganizationUnitAsync(int tenantId, SetOrganizationUnitRoleDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await CheckExistenceOfRoleAndOuAsync(input);

                await _roleManager.RemoveFromOrganizationUnitAsync(input.RoleId, input.OrganizationUnitId);
            }
        }

        public async Task AddUserToOrganizationAsync(int tenantId, SetOrganizationUnitUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await CheckExistenceOfUserAndOuAsync(input);

                await _userManager.AddToOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
            }
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

        public async Task RemoveUserToOrganizationAsync(int tenantId, SetOrganizationUnitUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await CheckExistenceOfUserAndOuAsync(input);

                await _userManager.RemoveFromOrganizationUnitAsync(input.UserId, input.OrganizationUnitId);
            }
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
