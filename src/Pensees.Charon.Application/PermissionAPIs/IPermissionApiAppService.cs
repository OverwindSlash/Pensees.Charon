﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Pensees.Charon.PermissionAPIs.Dto;

namespace Pensees.Charon.PermissionAPIs
{
    public interface IPermissionApiAppService : IApplicationService
    {
        public Task AddAnonymousRouteAsync(SetAnonymousDto setAnonymousDto);

        public Task RemoveAnonymousRouteAsync(SetAnonymousDto setAnonymousDto);

        public Task DoNoPermissionWork();

        public Task DoVideoViewPermissionWork();

        public Task DoServiceGetPermissionWork();

        public Task DoServicePostPermissionWork();
    }
}
