using APICore.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APICore.Data
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        // Use plural names for DbSet to follow common conventions
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Log> Logs { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditableEntityRules();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditableEntityRules()
        {
            var currentDate = DateTime.UtcNow; // prefer UTC for timestamps

            var entries = ChangeTracker.Entries<BaseEntity>().ToList();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = currentDate;
                        entry.Entity.ModifiedAt = currentDate;
                        break;

                    case EntityState.Modified:
                        // Don't overwrite CreatedAt on update - preserve original value
                        entry.Entity.ModifiedAt = currentDate;
                        if (entry.OriginalValues.TryGetValue<DateTime>("CreatedAt", out var originalCreatedAt))
                        {
                            entry.Entity.CreatedAt = originalCreatedAt;
                        }
                        break;

                    case EntityState.Detached:
                    case EntityState.Deleted:
                    case EntityState.Unchanged:
                        // no-op
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}