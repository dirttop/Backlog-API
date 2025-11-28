using Microsoft.EntityFrameworkCore;
using BacklogAPI.Models;

namespace BacklogAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetCompletedOnDates();
            return await base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges()
        {
            SetCompletedOnDates();
            return base.SaveChanges();
        }

        private void SetCompletedOnDates()
        {
            var entries = ChangeTracker
                .Entries<Game>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var game = entry.Entity;
                if (game.Completed)
                {
                    if (!game.CompletedOn.HasValue)
                    {
                        game.CompletedOn = now;
                    }
                }
                else
                {
                    game.CompletedOn = null;
                }
            }
        }
    }
}