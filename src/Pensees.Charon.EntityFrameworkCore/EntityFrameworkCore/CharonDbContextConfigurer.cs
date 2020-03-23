using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Pensees.Charon.EntityFrameworkCore
{
    public static class CharonDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<CharonDbContext> builder, string connectionString)
        {
            builder.UseMySql(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<CharonDbContext> builder, DbConnection connection)
        {
            builder.UseMySql(connection);
        }
    }
}
