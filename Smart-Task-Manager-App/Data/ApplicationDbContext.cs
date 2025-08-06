using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Models;

namespace SmartTaskManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<TaskLog> TaskLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Models.Task>(entity =>
            {
                entity.Property(t => t.Priority).HasConversion<string>();
                entity.Property(t => t.Status).HasConversion<string>();
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(t => t.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(t => t.AssignedUser)
                    .WithMany(u => u.Tasks)
                    .HasForeignKey(t => t.AssignedUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.Property(r => r.Period).HasConversion<string>();
                entity.Property(r => r.GeneratedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reports)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TaskLog>(entity =>
            {
                entity.Property(l => l.RescheduledAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(l => l.Task)
                    .WithMany()
                    .HasForeignKey(l => l.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.Property(rt => rt.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(rt => rt.Expires).IsRequired();
                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}