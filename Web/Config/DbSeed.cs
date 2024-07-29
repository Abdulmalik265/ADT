using Data;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Web.Config
{
    internal class DbSeed : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DbSeed(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeed>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Persona>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            //shared db migration
            var adtDbContext = scope.ServiceProvider.GetRequiredService<AdtDbContext>();
            try
            {
                logger.LogInformation("Applying Migration!");
                await adtDbContext.Database.MigrateAsync(cancellationToken: cancellationToken);
                logger.LogInformation("Migration Successful!");

                logger.LogInformation("Seeding  Roles");
                await roleManager.SeedRoles();
                logger.LogInformation("Seeding  Roles Successful!");

                logger.LogInformation("Seeding  Users");
                await userManager.SeedUsers();
                logger.LogInformation("Seeding  User Successful!");

                logger.LogInformation("Seeding States");
                await adtDbContext.SeedState();
                logger.LogInformation("State Seed Successfully!");

                logger.LogInformation("Seeding LocalGovernments");
                await adtDbContext.SeedLocalgovernment();
                logger.LogInformation("Wards Seed Successful!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while applying Access DB Migration!");
            }


        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
