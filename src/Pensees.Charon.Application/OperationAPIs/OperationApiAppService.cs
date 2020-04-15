using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Organizations;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pensees.Charon.Authorization;
using Pensees.Charon.Authorization.Roles;
using Pensees.Charon.Authorization.Users;
using Pensees.Charon.MultiTenancy;
using Pensees.Charon.MultiTenancy.Dto;
using Pensees.Charon.OperationAPIs.Dto;
using Pensees.Charon.Organizations.Dto;
using Pensees.Charon.Roles.Dto;
using Pensees.Charon.Users.Dto;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Pensees.Charon.OperationAPIs
{
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public class OperationApiAppService : ApplicationService, IOperationApiAppService
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly TenantManager _tenantManager;
        private readonly IFeatureManager _featureManager;
        private readonly IRepository<EditionFeatureSetting, long> _editionFeatureRepository;
        private readonly ILocalizationManager _localizationManager;
        private readonly IIocManager _iocManager;
        private readonly IAuthorizationConfiguration _authorizationConfiguration;
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly OrganizationUnitManager _organizationUnitManager;

        public OperationApiAppService(
            ITenantAppService tenantAppService,
            TenantManager tenantManager,
            IFeatureManager featureManager,
            IRepository<EditionFeatureSetting, long> editionFeatureRepository,
            ILocalizationManager localizationManager,
            IIocManager iocManager,
            IAuthorizationConfiguration authorizationConfiguration,
            RoleManager roleManager,
            UserManager userManager,
            IRepository<User, long> userRepository,
            IRepository<OrganizationUnit, long> orgUnitRepository,
            OrganizationUnitManager organizationUnitManager)
        {
            _tenantAppService = tenantAppService;
            _tenantManager = tenantManager;
            _featureManager = featureManager;
            _editionFeatureRepository = editionFeatureRepository;
            _localizationManager = localizationManager;
            _iocManager = iocManager;
            _authorizationConfiguration = authorizationConfiguration;
            _roleManager = roleManager;
            _userManager = userManager;
            _userRepository = userRepository;
            _orgUnitRepository = orgUnitRepository;
            _organizationUnitManager = organizationUnitManager;
        }

        #region Tenant Methods
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
        #endregion

        #region Feature & Permission Methods
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

            var allFeatures = _featureManager.GetAll();
            foreach (var feature in allFeatures)
            {
                if (input.FeatureNames.Contains(feature.Name))
                {
                    await _tenantManager.SetFeatureValueAsync(tenant.Id, feature.Name, "true");
                }
                else
                {
                    await _tenantManager.SetFeatureValueAsync(tenant.Id, feature.Name, "false");
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                // Grant all permissions to admin role
                var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);

                using (var featureDependencyContext = _iocManager.ResolveAsDisposable<FeatureDependencyContext>())
                {
                    var featureDependencyContextObject = featureDependencyContext.Object;
                    featureDependencyContextObject.TenantId = tenant.Id;

                    var permissions = PermissionManager.GetAllPermissions(adminRole.GetMultiTenancySide())
                        .Where(permission =>
                            permission.FeatureDependency == null ||
                            permission.FeatureDependency.IsSatisfied(featureDependencyContextObject)
                        ).ToList();

                    await _roleManager.SetGrantedPermissionsAsync(adminRole, permissions);
                }
            }

            return true;                                                                                                         
        }

        public async Task<List<FeatureDto>> ListAllFeaturesInTenant(int tenantId)
        {
            var features = await _tenantManager.GetFeatureValuesAsync(tenantId);

            List<FeatureDto> featureDtos = new List<FeatureDto>();
            foreach (var feature in features)
            {
                if (feature.Value == "true")
                {
                    Feature entity = _featureManager.Get(feature.Name);
                    featureDtos.Add(new FeatureDto()
                    {
                        Name = entity.Name,
                        DisplayName = _localizationManager.GetString((LocalizableString)entity.DisplayName)
                    });
                }
            }

            return featureDtos;
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
        #endregion

        #region Role Methods
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

        public async Task<RoleDto> GetRoleInTenantAsync(int tenantId, EntityDto<int> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var entity = await _roleManager.GetRoleByIdAsync(input.Id);
                    return ObjectMapper.Map<RoleDto>(entity);
                }
                catch (Exception exception)
                {
                    throw new UserFriendlyException(exception.Message);
                }
            }
        }

        public async Task<ListResultDto<RoleListDto>> GetAllRolesInTenantAsync(int tenantId, GetRolesInput input)
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
        #endregion

        #region Organization Methods
        public async Task<OrganizationUnitDto> CreateOuInTenantAsync(CreateOrganizationUnitDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                if (string.IsNullOrEmpty(input.Code))
                {
                    input.Code = await _organizationUnitManager.GetNextChildCodeAsync(input.ParentId);
                }

                OrganizationUnit entity = ObjectMapper.Map<OrganizationUnit>(input);

                //await _organizationUnitManager.CreateAsync(entity);

                long id = await _orgUnitRepository.InsertAndGetIdAsync(entity);
                entity.Id = id;

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

        public async Task<PagedResultDto<OrganizationUnitDto>> GetAllOusInTenantAsync(int tenantId, PagedResultRequestDto input)
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
        #endregion

        #region User Methods
        public async Task<UserDto> CreateUserInTenantAsync(int tenantId, CreateUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = ObjectMapper.Map<User>(input);

                user.TenantId = AbpSession.TenantId;
                user.IsEmailConfirmed = true;

                await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

                CheckErrors(await _userManager.CreateAsync(user, input.Password));

                // if (input.RoleNames != null)
                // {
                //     CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                // }

                foreach (string orgUnitName in input.OrgUnitNames)
                {
                    OrganizationUnit ou = _orgUnitRepository.FirstOrDefault(
                        ou => ou.DisplayName.ToLower() == orgUnitName.ToLower());

                    if (ou == null)
                    {
                        continue;
                    }

                    await AddUserToOuAndSetRoleAsync(user, ou);
                }

                CurrentUnitOfWork.SaveChanges();

                return ObjectMapper.Map<UserDto>(user);
            }
        }

        public async Task<UserDto> CreateAdminUserInTenantAsync(int tenantId, CreateUserDto input)
        {
            input.OrgUnitNames = input.OrgUnitNames.Append("AdminGroup").ToArray();

            return await CreateUserInTenantAsync(tenantId, input);
        }

        public async Task<List<UserDto>> GetAllAdminUserInTenantAsync(int tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {

                var adminRole = await _roleManager.GetRoleByNameAsync(StaticRoleNames.Tenants.Admin);

                IList<User> adminUsers = await _userManager.GetUsersInRoleAsync(adminRole.Name);

                return ObjectMapper.Map<List<UserDto>>(adminUsers);
            }
        }

        private async Task AddUserToOuAndSetRoleAsync(User user, OrganizationUnit ou)
        {
            await _userManager.AddToOrganizationUnitAsync(user, ou);

            var roles = await _roleManager.GetRolesInOrganizationUnit(ou);
            CheckErrors(await _userManager.AddToRolesAsync(user, roles.Select(r => r.NormalizedName).ToArray()));
        }

        public async Task<UserDto> GetUserInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = await _userManager.GetUserByIdAsync(input.Id);

                UserDto dto = ObjectMapper.Map<UserDto>(user);

                IList<string> roles = await _userManager.GetRolesAsync(user);
                dto.RoleNames = roles.ToArray();

                //List<Permission> permissions = new List<Permission>();
                //foreach (string roleName in dto.RoleNames)
                //{
                //    Role role = await _roleManager.GetRoleByNameAsync(roleName);
                //    permissions.AddRange(await _roleManager.GetGrantedPermissionsAsync(role.Id));
                //}

                return dto;
            }
        }

        public async Task<UserDto> UpdateUserInTenantAsync(int tenantId, UserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = await _userManager.GetUserByIdAsync(input.Id);

                ObjectMapper.Map(input, user);
                user.SetNormalizedNames();

                CheckErrors(await _userManager.UpdateAsync(user));

                // if (input.RoleNames != null)
                // {
                //     CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
                // }

                if (input.OrgUnitNames != null)
                {
                    //var ous = await _userManager.GetOrganizationUnitsAsync(user);

                    var ous = _orgUnitRepository.GetAll()
                        .Where(ou => input.OrgUnitNames.Contains(ou.DisplayName)).ToList();

                    foreach (OrganizationUnit ou in ous)
                    {
                        await AddUserToOuAndSetRoleAsync(user, ou);
                    }
                }

                return await GetUserInTenantAsync(tenantId, input);
            }
        }

        public async Task DeleteUserInTenantAsync(int tenantId, EntityDto<long> input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = await _userManager.GetUserByIdAsync(input.Id);
                await _userManager.DeleteAsync(user);
            }
        }

        public async Task<bool> ActivateUserInTenantAsync(int tenantId, ActivateUserDto input)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = await _userManager.GetUserByIdAsync(input.UserId);
                if (user == null)
                {
                    return false;
                }

                user.IsActive = input.IsActive;

                CheckErrors(await _userManager.UpdateAsync(user));

                return true;
            }
        } 
        #endregion

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
