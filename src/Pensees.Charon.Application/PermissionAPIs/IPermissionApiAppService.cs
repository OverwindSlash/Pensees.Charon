using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;

namespace Pensees.Charon.PermissionAPIs
{
    public interface IPermissionApiAppService : IApplicationService
    {
        public Task DoNoPermissionWork();

        public Task DoVideoViewPermissionWork();

        public Task DoServiceGetPermissionWork();

        public Task DoServicePostPermissionWork();
    }
}
