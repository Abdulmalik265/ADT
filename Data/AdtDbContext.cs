using Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Data
{
    public class AdtDbContext : IdentityDbContext<Persona, Role, Guid>
    {
        public AdtDbContext(DbContextOptions<AdtDbContext> options ) : base( options ) 
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<LocalGovernment> localGovernments { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<PaymentInfo> PaymentInfos { get; set; }

        public DbSet<Director> Directors { get; set; }



        public async Task<bool> TrySaveChangesAsync()
        {
            try
            {
                await SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
