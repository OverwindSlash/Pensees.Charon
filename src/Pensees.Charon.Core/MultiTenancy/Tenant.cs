using Abp.MultiTenancy;
using Pensees.Charon.Authorization.Users;
using System.ComponentModel.DataAnnotations;

namespace Pensees.Charon.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public const int MaxLogoLength = 128;
        public const int MaxContactLength = 128;
        public const int MaxPhoneNumberLength = 32;
        public const int MaxAddressLength = 256;

        [StringLength(MaxLogoLength)]
        public string Logo { get; set; }

        [StringLength(MaxContactLength)]
        public string Contact { get; set; }

        [StringLength(MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        [StringLength(MaxAddressLength)]
        public string Address { get; set; }

        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
