using Abp.Application.Services;
using Pensees.Charon.MultiTenancy.Dto;

namespace Pensees.Charon.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

