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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Identity).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(500); // Hash can be long
                entity.Property(e => e.Avatar).HasMaxLength(500);
                entity.Property(e => e.AvatarMimeType).HasMaxLength(100);

                // Configure enums to be stored as strings (more readable in DB)
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(10);

                // Create unique index on Email for performance and uniqueness
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Identity).IsUnique();
            });

            // Configure UserToken entity
            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AccessToken).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.RefreshToken).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.DeviceBrand).HasMaxLength(100);
                entity.Property(e => e.DeviceModel).HasMaxLength(100);
                entity.Property(e => e.OS).HasMaxLength(50);
                entity.Property(e => e.OSPlatform).HasMaxLength(50);
                entity.Property(e => e.OSVersion).HasMaxLength(50);
                entity.Property(e => e.ClientName).HasMaxLength(100);
                entity.Property(e => e.ClientType).HasMaxLength(50);
                entity.Property(e => e.ClientVersion).HasMaxLength(50);

                // Configure foreign key relationship
                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserTokens)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index on UserId for performance
                entity.HasIndex(e => e.UserId);
            });

            // Configure Setting entity
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Value).HasMaxLength(2000); // Allow longer values

                // Create unique index on Key to prevent duplicate settings
                entity.HasIndex(e => e.Key).IsUnique();
            });

            // Configure Log entity
            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.App).HasMaxLength(100);
                entity.Property(e => e.Module).HasMaxLength(100);

                // Configure enums to be stored as strings
                entity.Property(e => e.EventType).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.LogType).HasConversion<string>().HasMaxLength(50);

                // Index on CreatedAt for performance (logs are often queried by date)
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.UserId);
            });

            // Configure BaseEntity properties for all entities that inherit from it
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Ensure CreatedAt and ModifiedAt are required and have proper precision
                    var createdAtProperty = entityType.FindProperty(nameof(BaseEntity.CreatedAt));
                    var modifiedAtProperty = entityType.FindProperty(nameof(BaseEntity.ModifiedAt));

                    if (createdAtProperty != null)
                    {
                        createdAtProperty.SetColumnType("timestamp with time zone");
                    }

                    if (modifiedAtProperty != null)
                    {
                        modifiedAtProperty.SetColumnType("timestamp with time zone");
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }

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