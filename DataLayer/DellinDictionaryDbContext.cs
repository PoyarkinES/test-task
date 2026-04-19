using DataLayer.Model;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;

namespace DataLayer
{
    public class DellinDictionaryDbContext(DbContextOptions<DellinDictionaryDbContext> options) : DbContext(options)
    {
        public DbSet<Office> Offices { get; set; }
        public DbSet<Coordinates> Coordinateses{ get; set; }
        public DbSet<Phone> Phone { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("dbo");
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
                optionsBuilder.EnableSensitiveDataLogging(true);
        }
    }
}
