using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Authorization;

namespace Pensees.Charon.Roles.Dto
{
    [AutoMapFrom(typeof(Permission))]
    public class PermissionDto
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }
    }
}
