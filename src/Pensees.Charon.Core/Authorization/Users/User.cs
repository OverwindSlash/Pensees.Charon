using Abp.Authorization.Users;
using Abp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pensees.Charon.Authorization.Users
{
    public class User : AbpUser<User>
    {
        public const string DefaultPassword = "123qwe";

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            return user;
        }

        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string OfficePhoneNumber { get; set; }

        [MaxLength(2)]
        public string Gender { get; set; }

        [StringLength(18)]
        public string IdNumber { get; set; }

        [MaxLength(10)]
        public string City { get; set; }

        public DateTime ExpireDate { get; set; }
    }
}
