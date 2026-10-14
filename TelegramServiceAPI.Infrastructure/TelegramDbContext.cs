using Microsoft.EntityFrameworkCore;
using TelegramServiceAPI.Infrastructure.Entities;

namespace TelegramServiceAPI.Infrastructure
{
    public class TelegramDbContext(DbContextOptions<TelegramDbContext> options) : DbContext(options)
    {
        public DbSet<ConnectionEntity> Connections => Set<ConnectionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelegramDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
