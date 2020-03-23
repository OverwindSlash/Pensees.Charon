using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Pensees.Charon.Organizations.Dto;

namespace Pensees.Charon.Organizations
{
    public interface IOrganizationUnitAppService : IApplicationService
    {
        public Task AddRoleToOrganizationUnitAsync(SetOrganizationUnitRoleDto input);
        public Task RemoveRoleFromOrganizationUnitAsync(SetOrganizationUnitRoleDto input);

        public Task AddUserToOrganizationAsync(SetOrganizationUnitUserDto input);
        public Task RemoveUserToOrganizationAsync(SetOrganizationUnitUserDto input);
    }
}
