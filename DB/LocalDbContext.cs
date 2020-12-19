using System;
using CESystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CESystem.DB
{
    public class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        public LocalDbContext()
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { set; get; }
        public DbSet<Currency> Currencies { set; get; }
        public DbSet<Purse> Purses { set; get; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Name).IsUnique();

            modelBuilder
                .Entity<Currency>()
                .HasIndex(c => c.Name).IsUnique();

            modelBuilder
                .Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("client");
            
            modelBuilder
                .Entity<Purse>()
                .Property(p => p.CashValue)
                .HasDefaultValue(0.0);
            
            modelBuilder
                .Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .HasConstraintName("FK_id_user");

            modelBuilder
                .Entity<Currency>()
                .HasMany(a => a.Accounts)
                .WithMany(c => c.Currencies)
                .UsingEntity<Purse>(
                    purse => purse
                        .HasOne(p => p.Account)
                        .WithMany(a => a.Purses)
                        .HasForeignKey(p => p.IdAccount),
                    purse => purse
                        .HasOne(p => p.Currency)
                        .WithMany(a => a.Purses)
                        .HasForeignKey(p => p.IdCurrency),
            purse =>
                    {
                        purse.HasKey(p => new {p.IdAccount, p.IdCurrency});
                    });
        }
    }
}