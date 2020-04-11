using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Organizations;
using Abp.Runtime.Session;
using Abp.UI;
using Pensees.Charon.Authorization;
using Pensees.Charon.Authorization.Accounts;
using Pensees.Charon.Authorization.Roles;
using Pensees.Charon.Authorization.Users;
using Pensees.Charon.Roles.Dto;
using Pensees.Charon.Users.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pensees.Charon.Authorization.AuthCode;

namespace Pensees.Charon.Users
{
    [AbpAuthorize(PermissionNames.Pages_Users)]
    public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>, IUserAppService
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<OrganizationUnit, long> _orgUnitRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly LogInManager _logInManager;
        private readonly SmsAuthManager _smsAuthManager;

        public UserAppService(
            IRepository<User, long> repository,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<OrganizationUnit, long> orgUnitRepository,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            IUnitOfWorkManager unitOfWorkManager,
            LogInManager logInManager,
            SmsAuthManager smsAuthManager)
            : base(repository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _orgUnitRepository = orgUnitRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _unitOfWorkManager = unitOfWorkManager;
            _logInManager = logInManager;
            _smsAuthManager = smsAuthManager;
        }

        public override async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            CheckCreatePermission();

            var user = ObjectMapper.Map<User>(input);

            user.TenantId = AbpSession.TenantId;
            user.IsEmailConfirmed = true;

            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            CheckErrors(await _userManager.CreateAsync(user, input.Password));

            // if (input.RoleNames != null)
            // {
            //     CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            // }

            foreach (string orgUnitName in input.OrgUnitNames)
            {
                OrganizationUnit ou = _orgUnitRepository.FirstOrDefault(
                    ou => ou.DisplayName.ToLower() == orgUnitName.ToLower());

                if (ou == null)
                {
                    continue;
                }

                await AddUserToOuAndSetRoleAsync(user, ou);
            }

            CurrentUnitOfWork.SaveChanges();

            return MapToEntityDto(user);
        }

        public override async Task<UserDto> UpdateAsync(UserDto input)
        {
            CheckUpdatePermission();

            var user = await _userManager.GetUserByIdAsync(input.Id);

            MapToEntity(input, user);

            CheckErrors(await _userManager.UpdateAsync(user));

            // if (input.RoleNames != null)
            // {
            //     CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            // }

            if (input.OrgUnitNames != null)
            {
                //var ous = await _userManager.GetOrganizationUnitsAsync(user);

                var ous = _orgUnitRepository.GetAll()
                    .Where(ou => input.OrgUnitNames.Contains(ou.DisplayName)).ToList();

                foreach (OrganizationUnit ou in ous)
                {
                    await AddUserToOuAndSetRoleAsync(user, ou);
                }
            }

            return await GetAsync(input);
        }

        private async Task AddUserToOuAndSetRoleAsync(User user, OrganizationUnit ou)
        {
            await _userManager.AddToOrganizationUnitAsync(user, ou);

            var roles = await _roleManager.GetRolesInOrganizationUnit(ou);
            CheckErrors(await _userManager.AddToRolesAsync(user, roles.Select(r => r.NormalizedName).ToArray()));
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var user = await _userManager.GetUserByIdAsync(input.Id);
            await _userManager.DeleteAsync(user);
        }

        public async Task<ListResultDto<RoleDto>> GetRoles()
        {
            var roles = await _roleRepository.GetAllListAsync();
            return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        }

        public async Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }

        [HttpPost]
        [AbpAllowAnonymous]
        public async Task<ListResultDto<string>> GetPermissions(GetPermissionsDto input)
        {
            using (_unitOfWorkManager.Current.SetTenantId(input.TenantId))
            {
                var user = await Repository.GetAllIncluding(
                    x => x.Roles).FirstOrDefaultAsync(x => x.Id == input.UserId);

                List<Permission> permissions = new List<Permission>();

                foreach (UserRole role in user.Roles)
                {
                    permissions.AddRange(await _roleManager.GetGrantedPermissionsAsync(role.RoleId));
                }

                permissions = permissions.Distinct().ToList();

                return new ListResultDto<string>(permissions.Select(p => p.Name).ToList());
            }
        }

        [HttpPut]
        public async Task<bool> ActivateUser(ActivateUserDto input)
        {
            var user = await Repository.GetAsync(input.UserId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = input.IsActive;

            CheckErrors(await _userManager.UpdateAsync(user));

            return true;
        }

        protected override User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();
            return user;
        }

        protected override void MapToEntity(UserDto input, User user)
        {
            ObjectMapper.Map(input, user);
            user.SetNormalizedNames();
        }

        protected override UserDto MapToEntityDto(User user)
        {
            var userDto = base.MapToEntityDto(user);

            if (user.Roles != null)
            {
                var roleIds = user.Roles.Select(x => x.RoleId).ToArray();

                var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

                userDto.RoleNames = roles.ToArray();
            }
            

            var ous = _userManager.GetOrganizationUnits(user).Select(ou => ou.DisplayName);
            userDto.OrgUnitNames = ous.ToArray();

            return userDto;
        }

        protected override IQueryable<User> CreateFilteredQuery(PagedUserResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Roles)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.UserName.Contains(input.Keyword) || x.Name.Contains(input.Keyword) || x.EmailAddress.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            return user;
        }

        protected override IQueryable<User> ApplySorting(IQueryable<User> query, PagedUserResultRequestDto input)
        {
            return query.OrderBy(r => r.UserName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to change password.");
            }
            long userId = _abpSession.UserId.Value;
            var user = await _userManager.GetUserByIdAsync(userId);
            var loginAsync = await _logInManager.LoginAsync(user.UserName, input.CurrentPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Existing Password' did not match the one on record.  Please try again or contact an administrator for assistance in resetting your password.");
            }
            if (!new Regex(AccountAppService.PasswordRegex).IsMatch(input.NewPassword))
            {
                throw new UserFriendlyException("Passwords must be at least 8 characters, contain a lowercase, uppercase, and number.");
            }
            user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
            CurrentUnitOfWork.SaveChanges();
            return true;
        }

        public async Task<bool> ResetPasswordByAdmin(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attemping to reset password.");
            }

            long currentUserId = _abpSession.UserId.Value;
            var currentUser = await _userManager.GetUserByIdAsync(currentUserId);
            var loginAsync = await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Admin Password' did not match the one on record.  Please try again.");
            }
            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }
            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException("Only administrators may reset passwords.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                CurrentUnitOfWork.SaveChanges();
            }

            return true;
        }

        public async Task<bool> ResetSelfPasswordBySms(SmsResetPasswordDto input)
        {
            if (! await _smsAuthManager.AuthenticateSmsCode(input.PhoneNumber, input.AutoCode))
            {
                throw new UserFriendlyException("Wrong authentication code.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user == null)
            {
                throw new UserFriendlyException("User not exist.");
            }

            if (user.PhoneNumber != input.PhoneNumber)
            {
                throw new UserFriendlyException("Wrong mobile phone number.");
            }

            user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
            CurrentUnitOfWork.SaveChanges();
            return true;
        }
    }
}

