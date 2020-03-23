using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using Pensees.Charon.Authorization.Roles;
using Pensees.Charon.Authorization.Users;
using Pensees.Charon.MultiTenancy;

namespace Pensees.Charon.EntityFrameworkCore
{
    public class CharonDbContext : AbpZeroDbContext<Tenant, Role, User, CharonDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public CharonDbContext(DbContextOptions<CharonDbContext> options)
            : base(options)
        {
        }
    }
}
