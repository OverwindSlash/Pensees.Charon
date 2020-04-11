using Abp.Application.Features;
using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;
using Pensees.Charon.Features;

namespace Pensees.Charon.Authorization
{
    public class CharonAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            // UI
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

            // Business
            var smartSecurityPermission = context.CreatePermission("SmartSecurity", 
                featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartSecurityFeature));

            var smartPassPermission = context.CreatePermission("SmartPass",
                featureDependency: new SimpleFeatureDependency(PesCloudFeatureProvider.SmartPassFeature));

            // Service Invoke
            var servicePermission = context.CreatePermission("ServiceInvoke");
            //servicePermission.CreateChildPermission("ServiceInvoke.Get");
            //servicePermission.CreateChildPermission("ServiceInvoke.Post");
            //servicePermission.CreateChildPermission("ServiceInvoke.Put");
            //servicePermission.CreateChildPermission("ServiceInvoke.Delete");

            // Video Control
            var videoPermission = context.CreatePermission("VideoControl");
            //videoPermission.CreateChildPermission("VideoControl.View");
            //videoPermission.CreateChildPermission("VideoControl.Copy");
            //videoPermission.CreateChildPermission("VideoControl.Delete");
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, CharonConsts.LocalizationSourceName);
        }
    }
}
