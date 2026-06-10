using Microsoft.EntityFrameworkCore;
using Rag.Services.Backend.Domain.Models;

namespace Rag.Services.Backend.Infrastructure.Extensions.EfCore
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageSource> MessageSources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(static entity =>
            {
                entity.HasKey(static e => e.Id);
                entity.HasIndex(static e => e.GoogleId).IsUnique();
                entity.HasIndex(static e => e.Email);
                entity.Property(static e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(static e => e.GoogleId).IsRequired().HasMaxLength(255);
                entity.Property(static e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(static e => e.PictureUrl).HasMaxLength(500);
            });

            // Conversation configuration
            modelBuilder.Entity<Conversation>(static entity =>
            {
                entity.HasKey(static e => e.Id);
                entity.HasIndex(static e => e.ConversationId).IsUnique();
                entity.HasIndex(static e => new { e.UserId, e.UpdatedAt });
                entity.Property(static e => e.ConversationId).IsRequired().HasMaxLength(50);
                entity.Property(static e => e.Title).IsRequired().HasMaxLength(500);

                entity.HasOne(static e => e.User)
                    .WithMany(static u => u.Conversations)
                    .HasForeignKey(static e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Message configuration
            modelBuilder.Entity<Message>(static entity =>
            {
                entity.HasKey(static e => e.Id);
                entity.HasIndex(static e => new { e.ConversationId, e.CreatedAt });
                entity.Property(static e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(static e => e.Content).IsRequired();

                entity.HasOne(static e => e.Conversation)
                    .WithMany(static c => c.Messages)
                    .HasForeignKey(static e => e.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MessageSource configuration
            modelBuilder.Entity<MessageSource>(static entity =>
            {
                entity.HasKey(static e => e.Id);
                entity.HasIndex(static e => e.MessageId);
                entity.Property(static e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(static e => e.Url).IsRequired().HasMaxLength(1000);
                entity.Property(static e => e.Excerpt).HasMaxLength(2000);

                entity.HasOne(static e => e.Message)
                    .WithMany(static m => m.Sources)
                    .HasForeignKey(static e => e.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}