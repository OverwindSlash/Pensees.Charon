using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pensees.Charon.Configuration;
using Pensees.Charon.Web;

namespace Pensees.Charon.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class CharonDbContextFactory : IDesignTimeDbContextFactory<CharonDbContext>
    {
        public CharonDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CharonDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            CharonDbContextConfigurer.Configure(builder, configuration.GetConnectionString(CharonConsts.ConnectionStringName));

            return new CharonDbContext(builder.Options);
        }
    }
}
