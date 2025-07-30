using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DibatechLinkerAPI.Models.Domain;

namespace DibatechLinkerAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ParsedLink> ParsedLinks { get; set; }
        public DbSet<SavedLink> SavedLinks { get; set; }
        public DbSet<UserFolder> UserFolders { get; set; }
        public DbSet<ReminderJob> ReminderJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ParsedLink
            modelBuilder.Entity<ParsedLink>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalUrl).IsRequired().HasMaxLength(2048);
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.ImageUrl).HasMaxLength(2048);
                entity.Property(e => e.Author).HasMaxLength(200);
                entity.Property(e => e.SiteName).HasMaxLength(200);
                entity.Property(e => e.Domain).HasMaxLength(200);
                entity.Property(e => e.ContentType).HasConversion<string>();
                entity.Property(e => e.Category).HasConversion<string>();
                entity.HasIndex(e => e.OriginalUrl);
                entity.HasIndex(e => e.Domain);
                entity.HasIndex(e => e.Category);
            });

            // Configure SavedLink
            modelBuilder.Entity<SavedLink>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomNote).HasMaxLength(1000);
                entity.Property(e => e.CustomTitle).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.Tags).HasMaxLength(1000);
                entity.Property(e => e.ShareToken).HasMaxLength(100);
                
                entity.HasOne(e => e.ParsedLink)
                    .WithMany(p => p.SavedLinks)
                    .HasForeignKey(e => e.ParsedLinkId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.SavedLinks)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Folder)
                    .WithMany(f => f.SavedLinks)
                    .HasForeignKey(e => e.FolderId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => e.ShareToken);
                entity.HasIndex(e => e.SavedAt);
                entity.HasIndex(e => e.Status);
            });

            // Configure UserFolder
            modelBuilder.Entity<UserFolder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Color).HasMaxLength(7); // Hex color code

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Folders)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
            });

            // Configure ReminderJob
            modelBuilder.Entity<ReminderJob>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Frequency).HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<ReminderJob>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.NextRunAt);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.ReminderFrequency).HasConversion<string>();
                entity.Property(e => e.RefreshToken).HasMaxLength(500);
            });
        }
    }
}
