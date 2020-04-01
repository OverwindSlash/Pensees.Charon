using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Pensees.Charon.Authorization.Users;
using System.Linq;

namespace Pensees.Charon.Sessions.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserLoginInfoDto : EntityDto<long>
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }

        public string[] Roles { get; set; }

        public string[] OrgUnits { get; set; }

        public string[] Permissions { get; set; }

        public bool IsAdmin { get => Roles.Contains("ADMIN"); }
    }
}
