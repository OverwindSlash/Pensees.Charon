using System.Threading.Tasks;
using Abp.Application.Services;
using Pensees.Charon.Sessions.Dto;

namespace Pensees.Charon.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
