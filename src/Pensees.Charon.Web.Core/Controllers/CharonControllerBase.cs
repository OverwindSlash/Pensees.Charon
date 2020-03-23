using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Pensees.Charon.Controllers
{
    public abstract class CharonControllerBase: AbpController
    {
        protected CharonControllerBase()
        {
            LocalizationSourceName = CharonConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
