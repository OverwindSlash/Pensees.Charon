using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Microsoft.EntityFrameworkCore.Internal;
using Pensees.Charon.Authorization.Users;

namespace Pensees.Charon.Users.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserDto : EntityDto<long>
    {
        private const string EmailDomain = @"@pensees.ai";
        private string _emailAddress;

        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [MaxLength(2)]
        public string Gender { get; set; }

        [StringLength(18)]
        public string IdNumber { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string OfficePhoneNumber { get; set; }

        [MaxLength(10)]
        public string City { get; set; }

        public DateTime ExpireDate { get; set; }

        //[Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress
        {
            get => string.IsNullOrEmpty(_emailAddress) ? (PhoneNumber + EmailDomain) : _emailAddress;
            set => _emailAddress = value;
        }

        public bool IsActive { get; set; }

        public string FullName { get; set; }

        public DateTime? LastLoginTime { get; set; }

        public DateTime CreationTime { get; set; }

        public string[] RoleNames { get; set; }

        public string[] OrgUnitNames { get; set; }

        public bool IsAdmin
        {
            get
            {
                if (RoleNames != null)
                {
                    return RoleNames.Contains("ADMIN");
                }

                return false;
            }

        }
    }
}
