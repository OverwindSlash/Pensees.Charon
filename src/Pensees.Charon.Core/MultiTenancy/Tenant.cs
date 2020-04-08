using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Pensees.Charon.Authorization.Users;

namespace Pensees.Charon.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        [StringLength(AbpTenantBase.MaxNameLength)]
        public string Logo { get; set; }

        [StringLength(AbpTenantBase.MaxNameLength)]
        public string Contact { get; set; }

        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
