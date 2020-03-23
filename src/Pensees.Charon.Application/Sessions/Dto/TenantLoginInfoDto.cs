using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Pensees.Charon.MultiTenancy;

namespace Pensees.Charon.Sessions.Dto
{
    [AutoMapFrom(typeof(Tenant))]
    public class TenantLoginInfoDto : EntityDto
    {
        public string TenancyName { get; set; }

        public string Name { get; set; }
    }
}
