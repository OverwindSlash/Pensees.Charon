using System.Threading.Tasks;
using Pensees.Charon.Configuration.Dto;

namespace Pensees.Charon.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
