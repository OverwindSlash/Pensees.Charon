using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Pensees.Charon.Configuration.Dto;

namespace Pensees.Charon.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : CharonAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
