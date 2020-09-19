using Microsoft.EntityFrameworkCore;

namespace Drone.Configuration.Database
{
    /**
     * Default context for configuration values.
     */
    public class ConfigurationEntityContext : DbContext
    {
        public ConfigurationEntityContext(DbContextOptions<ConfigurationEntityContext> options) : base(options)
        {

        }

        public DbSet<ConfigurationEntity> ConfigurationEntity { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ConfigurationEntity>().HasKey(o => o.Id);
        }
    }
}