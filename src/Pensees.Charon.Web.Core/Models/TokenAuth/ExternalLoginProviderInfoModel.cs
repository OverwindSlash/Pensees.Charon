using Abp.AutoMapper;
using Pensees.Charon.Authentication.External;

namespace Pensees.Charon.Models.TokenAuth
{
    [AutoMapFrom(typeof(ExternalLoginProviderInfo))]
    public class ExternalLoginProviderInfoModel
    {
        public string Name { get; set; }

        public string ClientId { get; set; }
    }
}
