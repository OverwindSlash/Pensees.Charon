using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Pensees.Charon.Authorization.Users;
using Pensees.Charon.Editions;

namespace Pensees.Charon.MultiTenancy
{
    public class TenantManager : AbpTenantManager<Tenant, User>
    {
        public TenantManager(
            IRepository<Tenant> tenantRepository, 
            IRepository<TenantFeatureSetting, long> tenantFeatureRepository, 
            EditionManager editionManager,
            IAbpZeroFeatureValueStore featureValueStore) 
            : base(
                tenantRepository, 
                tenantFeatureRepository, 
                editionManager,
                featureValueStore)
        {
        }
    }
}
