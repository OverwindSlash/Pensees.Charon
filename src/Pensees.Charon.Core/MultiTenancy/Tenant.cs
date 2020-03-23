using Abp.MultiTenancy;
using Pensees.Charon.Authorization.Users;

namespace Pensees.Charon.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
