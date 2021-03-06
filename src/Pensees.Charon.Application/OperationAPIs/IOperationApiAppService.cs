﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Pensees.Charon.MultiTenancy.Dto;
using Pensees.Charon.OperationAPIs.Dto;
using Pensees.Charon.Organizations.Dto;
using Pensees.Charon.Roles.Dto;
using Pensees.Charon.Users.Dto;

namespace Pensees.Charon.OperationAPIs
{
    public interface IOperationApiAppService : IApplicationService
    {
        public Task<TenantDto> CreateTenantAsync(CreateTenantDto input);
        public Task<TenantDto> GetTenantAsync(EntityDto<int> input);
        public Task<PagedResultDto<TenantDto>> GetAllTenantAsync(PagedTenantResultRequestDto input);
        public Task<TenantDto> UpdateTenantAsync(TenantDto input);
        public Task DeleteTenantAsync(EntityDto<int> input);
        public Task<bool> ActivateTenantAsync(ActivateTenantDto input);
        
        public List<FeatureDto> ListAllFeatures();
        public Task<bool> EnableFeatureForTenantAsync(EnableFeatureDto input);
        public Task<List<FeatureDto>> ListAllFeaturesInTenant(int tenantId);
        public ListResultDto<PermissionDto> GetTenantPermissions(int tenantId);
        
        public Task<RoleDto> CreateRoleInTenantAsync(int tenantId, CreateRoleDto input);
        public Task<RoleDto> GetRoleInTenantAsync(int tenantId, EntityDto<int> input);
        public Task<ListResultDto<RoleListDto>> GetAllRolesInTenantAsync(int tenantId, GetRolesInput input);
        public Task<RoleDto> UpdateRoleInTenantAsync(int tenantId, RoleDto input);
        public Task DeleteRoleInTenantAsync(int tenantId, EntityDto<int> input);
        
        public Task<OrganizationUnitDto> CreateOuInTenantAsync(CreateOrganizationUnitDto input);
        public Task<OrganizationUnitDto> GetOuInTenantAsync(int tenantId, EntityDto<long> input);
        public Task<PagedResultDto<OrganizationUnitDto>> GetAllOusInTenantAsync(int tenantId, PagedResultRequestDto input);
        public Task<OrganizationUnitDto> UpdateOuInTenantAsync(OrganizationUnitDto input);
        public Task DeleteOuInTenantAsync(int tenantId, EntityDto<long> input);

        public Task AddRoleToOuInTenantAsync(int tenantId, SetOrganizationUnitRoleDto input);
        public Task RemoveRoleFromOrganizationUnitAsync(int tenantId, SetOrganizationUnitRoleDto input);
        public Task AddUserToOrganizationAsync(int tenantId, SetOrganizationUnitUserDto input);
        public Task RemoveUserToOrganizationAsync(int tenantId, SetOrganizationUnitUserDto input);

        public Task<UserDto> CreateUserInTenantAsync(int tenantId, CreateUserDto input);
        public Task<UserDto> CreateAdminUserInTenantAsync(int tenantId, CreateUserDto input);
        public Task<List<UserDto>> GetAllAdminUserInTenantAsync(int tenantId);
        public Task<UserDto> GetUserInTenantAsync(int tenantId, EntityDto<long> input);
        public Task<UserDto> UpdateUserInTenantAsync(int tenantId, UserDto input);
        public Task DeleteUserInTenantAsync(int tenantId, EntityDto<long> input);
        public Task<bool> ActivateUserInTenantAsync(int tenantId, ActivateUserDto input);
    }
}
