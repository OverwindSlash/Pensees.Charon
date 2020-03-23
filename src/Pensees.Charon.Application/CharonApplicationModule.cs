using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Pensees.Charon.Authorization;

namespace Pensees.Charon
{
    [DependsOn(
        typeof(CharonCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class CharonApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<CharonAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(CharonApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
