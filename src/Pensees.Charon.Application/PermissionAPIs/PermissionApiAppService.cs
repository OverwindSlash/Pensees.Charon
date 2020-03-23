using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Authorization;

namespace Pensees.Charon.PermissionAPIs
{
    public class PermissionApiAppService : IPermissionApiAppService
    {
        [AbpAuthorize]
        public Task DoNoPermissionWork()
        {
            return Task.CompletedTask;
        }

        [AbpAuthorize("VideoControl.View")]
        public Task DoVideoViewPermissionWork()
        {
            return Task.CompletedTask;
        }

        [AbpAuthorize("ServiceInvoke.Get")]
        public Task DoServiceGetPermissionWork()
        {
            return Task.CompletedTask;
        }

        [AbpAuthorize("ServiceInvoke.Post")]
        public Task DoServicePostPermissionWork()
        {
            return Task.CompletedTask;
        }
    }
}
