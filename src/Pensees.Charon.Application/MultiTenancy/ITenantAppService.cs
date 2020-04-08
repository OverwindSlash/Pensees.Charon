using System.Threading.Tasks;
using Abp.Application.Services;
using Pensees.Charon.MultiTenancy.Dto;

namespace Pensees.Charon.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
        Task<bool> ActivateTenant(ActivateTenantDto input);
    }
}

