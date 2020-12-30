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

        public DbSet<UserRecord> UserRecords { get; set; }
        public DbSet<AccountRecord> AccountRecords { set; get; }
        public DbSet<CurrencyRecord> CurrencyRecords { set; get; }
        public DbSet<WalletRecord> WalletRecords { set; get; }
        public DbSet<OperationsHistoryRecord> OperationsHistoryRecords { set; get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRecord>()
                .HasIndex(u => u.Name).IsUnique();

            modelBuilder
                .Entity<CurrencyRecord>()
                .HasIndex(c => c.Name).IsUnique();

            modelBuilder
                .Entity<UserRecord>()
                .Property(u => u.Role)
                .HasDefaultValue("client");

            modelBuilder
                .Entity<WalletRecord>()
                .Property(p => p.CashValue)
                .HasDefaultValue(0.0);

            modelBuilder
                .Entity<CommissionRecord>()
                .HasCheckConstraint("correct_commission", "id_user not null || id_currency not null");
            
            
            modelBuilder
                .Entity<UserRecord>()
                .HasOne(u => u.CommissionRecord)
                .WithOne(c => c.UserRecord)
                .HasForeignKey<CommissionRecord>(c => c.UserId)
                .HasConstraintName("FK_id_user");

            modelBuilder
                .Entity<AccountRecord>()
                .HasOne(a => a.UserRecord)
                .WithMany(u => u.AccountRecords)
                .HasForeignKey(a => a.UserId)
                .HasConstraintName("FK_id_user");

            modelBuilder
                .Entity<OperationsHistoryRecord>()
                .HasOne(oh => oh.UserRecord)
                .WithMany(u => u.OperationsHistoryRecords)
                .HasForeignKey(u => u.UserId)
                .HasConstraintName("FK_id_user");
            
            
            modelBuilder
                .Entity<CurrencyRecord>()
                .HasOne(c => c.CommissionRecord)
                .WithOne(c => c.CurrencyRecord)
                .HasForeignKey<CommissionRecord>(c => c.CurrencyId)
                .HasConstraintName("FK_id_user");
            
            
            modelBuilder
                .Entity<CurrencyRecord>()
                .HasMany(a => a.AccountRecords)
                .WithMany(c => c.CurrencyRecords)
                .UsingEntity<WalletRecord>(
                    wallet => wallet
                        .HasOne(p => p.AccountRecord)
                        .WithMany(a => a.WalletRecords)
                        .HasForeignKey(p => p.IdAccount),
                    wallet => wallet
                        .HasOne(p => p.CurrencyRecord)
                        .WithMany(a => a.WalletRecords)
                        .HasForeignKey(p => p.IdCurrency),
            wallet =>
                    {
                        wallet.HasKey(p => new {p.IdAccount, p.IdCurrency});
                    });
        }
    }
}