using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Pensees.Charon.Organizations.Dto;

namespace Pensees.Charon.Organizations
{
    public interface IOrganizationUnitAppService : IApplicationService
    {
        public Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitDto input);
        public Task<OrganizationUnitDto> GetAsync(EntityDto<long> input);
        public Task<PagedResultDto<OrganizationUnitDto>> GetAllAsync(PagedResultRequestDto input);
        public Task<OrganizationUnitDto> UpdateAsync(OrganizationUnitDto input);
        public Task DeleteAsync(EntityDto<long> input);


        public Task AddRoleToOrganizationUnitAsync(SetOrganizationUnitRoleDto input);
        public Task RemoveRoleFromOrganizationUnitAsync(SetOrganizationUnitRoleDto input);

        public Task AddUserToOrganizationAsync(SetOrganizationUnitUserDto input);
        public Task RemoveUserToOrganizationAsync(SetOrganizationUnitUserDto input);
    }
}
