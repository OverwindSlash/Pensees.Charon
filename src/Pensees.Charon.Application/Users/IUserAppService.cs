using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Pensees.Charon.Roles.Dto;
using Pensees.Charon.Users.Dto;

namespace Pensees.Charon.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task<ListResultDto<RoleDto>> GetRoles();

        Task ChangeLanguage(ChangeUserLanguageDto input);

        Task<ListResultDto<string>> GetPermissions(GetPermissionsDto input);
    }
}
