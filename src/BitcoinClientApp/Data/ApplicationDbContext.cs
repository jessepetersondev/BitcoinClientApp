using Microsoft.EntityFrameworkCore;
using BitcoinClientApp.Models;

namespace BitcoinClientApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Wallet> Wallets => Set<Wallet>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Wallet configuration
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EncryptedPrivateKey).IsRequired();
                entity.Property(e => e.Salt).IsRequired();
                entity.Property(e => e.PublicKey).IsRequired();
                entity.Property(e => e.Address).IsRequired();
                entity.HasIndex(e => e.Username);
                entity.HasIndex(e => e.Address);
            });
        }
    }
} 