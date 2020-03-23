using System.Threading.Tasks;
using Abp.Application.Services;
using Pensees.Charon.Authorization.Accounts.Dto;

namespace Pensees.Charon.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
