using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Organizations;

namespace Pensees.Charon.Organizations.Dto
{
    [AutoMap(typeof(OrganizationUnit))]
    public class OrganizationUnitDto : EntityDto<long>
    {
        public int? TenantId { get; set; }

        public long? ParentId { get; set; }

        public string DisplayName { get; set; }

        public string Code { get; set; }
    }
}
