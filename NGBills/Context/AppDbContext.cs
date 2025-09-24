using Microsoft.EntityFrameworkCore;
using NGBills.Entities;
using NGBills.Enum;

namespace NGBills.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<UtilityBill> UtilityBills { get; set; }
        public DbSet<UtilityProvider> UtilityProviders { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.PasswordSalt).IsRequired();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // One-to-One relationship with Wallet
                entity.HasOne(u => u.Wallet)
                      .WithOne(w => w.User)
                      .HasForeignKey<Wallet>(w => w.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // One-to-Many relationship with UtilityBills - FIXED
                entity.HasMany(u => u.UtilityBills)
                      .WithOne(ub => ub.User)
                      .HasForeignKey(ub => ub.UserId)
                      .OnDelete(DeleteBehavior.ClientCascade);
            });

            // Wallet Configuration
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.HasIndex(w => w.UserId).IsUnique();

                entity.Property(w => w.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(w => w.Currency).HasMaxLength(3).HasDefaultValue("NGN");
                entity.Property(w => w.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(w => w.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // One-to-Many relationship with Transactions
                entity.HasMany(w => w.Transactions)
                      .WithOne(t => t.Wallet)
                      .HasForeignKey(t => t.WalletId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Transaction Configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.Reference).IsUnique();

                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Reference).IsRequired().HasMaxLength(100);
                entity.Property(t => t.Description).HasMaxLength(500);
                entity.Property(t => t.BillType).HasMaxLength(50);
                entity.Property(t => t.MeterNumber).HasMaxLength(50);
                entity.Property(t => t.CustomerName).HasMaxLength(200);
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(t => t.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Many-to-One relationship with Wallet
                entity.HasOne(t => t.Wallet)
                      .WithMany(w => w.Transactions)
                      .HasForeignKey(t => t.WalletId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UtilityProvider Configuration
            modelBuilder.Entity<UtilityProvider>(entity =>
            {
                entity.HasKey(up => up.Id);
                entity.HasIndex(up => up.Name).IsUnique();
                entity.HasIndex(up => up.Code).IsUnique();

                entity.Property(up => up.Name).IsRequired().HasMaxLength(200);
                entity.Property(up => up.Code).IsRequired().HasMaxLength(20);
                entity.Property(up => up.Description).HasMaxLength(500);
                entity.Property(up => up.ApiEndpoint).HasMaxLength(500);
                entity.Property(up => up.AccountNumberFormat).HasMaxLength(100);
                entity.Property(up => up.IsActive).HasDefaultValue(true);
                entity.Property(up => up.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(up => up.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // One-to-Many relationship with UtilityBills
                entity.HasMany(up => up.Bills)
                      .WithOne(ub => ub.UtilityProvider)
                      .HasForeignKey(ub => ub.ProviderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // UtilityBill Configuration - FIXED
            modelBuilder.Entity<UtilityBill>(entity =>
            {
                entity.HasKey(ub => ub.Id);

                entity.Property(ub => ub.BillType).IsRequired().HasMaxLength(50);
                entity.Property(ub => ub.Provider).IsRequired().HasMaxLength(200);
                entity.Property(ub => ub.Amount).HasColumnType("decimal(18,2)");
                entity.Property(ub => ub.PreviousBalance).HasColumnType("decimal(18,2)");
                entity.Property(ub => ub.CurrentCharge).HasColumnType("decimal(18,2)");
                entity.Property(ub => ub.AccountNumber).HasMaxLength(50);
                entity.Property(ub => ub.BillReference).HasMaxLength(100);
                entity.Property(ub => ub.RetrievalReference).HasMaxLength(100);
                entity.Property(ub => ub.IsPaid).HasDefaultValue(false);
                entity.Property(ub => ub.IsDeleted).HasDefaultValue(false);
                entity.Property(ub => ub.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(ub => ub.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Index for better query performance
                entity.HasIndex(ub => new { ub.UserId, ub.IsPaid, ub.DueDate });
                entity.HasIndex(ub => ub.BillReference).IsUnique();
                entity.HasIndex(ub => ub.RetrievalReference).IsUnique();

                // Many-to-One relationship with User - FIXED
                entity.HasOne(ub => ub.User)
                      .WithMany(u => u.UtilityBills)
                      .HasForeignKey(ub => ub.UserId)
                      .OnDelete(DeleteBehavior.ClientCascade);

                // Many-to-One relationship with UtilityProvider
                entity.HasOne(ub => ub.UtilityProvider)
                      .WithMany(up => up.Bills)
                      .HasForeignKey(ub => ub.ProviderId)
                      .OnDelete(DeleteBehavior.Restrict);

                // One-to-One relationship with Transaction
                entity.HasOne(ub => ub.Transaction)
                      .WithOne()
                      .HasForeignKey<UtilityBill>(ub => ub.TransactionId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed initial utility providers
            SeedUtilityProviders(modelBuilder);
        }

        private void SeedUtilityProviders(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UtilityProvider>().HasData(
                new UtilityProvider
                {
                    Id = 1,
                    Name = "IKEDC Electricity",
                    Code = "IKEDC",
                    Type = BillType.Electricity,
                    Description = "Ikeja Electric Distribution Company",
                    ApiEndpoint = "https://api.ikedc.com/v1",
                    RequiresAccountValidation = true,
                    AccountNumberFormat = "^IKEDC\\d{8}$",
                    IsActive = true
                },
                new UtilityProvider
                {
                    Id = 2,
                    Name = "LWC Water",
                    Code = "LWC",
                    Type = BillType.Water,
                    Description = "Lagos Water Corporation",
                    ApiEndpoint = "https://api.lwc.com/v1",
                    RequiresAccountValidation = true,
                    AccountNumberFormat = "^LWC\\d{7}$",
                    IsActive = true
                },
                new UtilityProvider
                {
                    Id = 3,
                    Name = "MTN Internet",
                    Code = "MTN",
                    Type = BillType.Internet,
                    Description = "MTN Nigeria Internet Services",
                    ApiEndpoint = "https://api.mtn.com/v1",
                    RequiresAccountValidation = true,
                    AccountNumberFormat = "^MTN\\d{10}$",
                    IsActive = true
                },
                new UtilityProvider
                {
                    Id = 4,
                    Name = "DSTV Cable TV",
                    Code = "DSTV",
                    Type = BillType.CableTV,
                    Description = "DSTV Cable Television",
                    ApiEndpoint = "https://api.dstv.com/v1",
                    RequiresAccountValidation = true,
                    AccountNumberFormat = "^DSTV\\d{9}$",
                    IsActive = true
                },
                new UtilityProvider
                {
                    Id = 5,
                    Name = "AIRTEL Airtime",
                    Code = "AIRTEL",
                    Type = BillType.Airtime,
                    Description = "Airtel Nigeria Airtime",
                    ApiEndpoint = "https://api.airtel.com/v1",
                    RequiresAccountValidation = false,
                    AccountNumberFormat = "^0[7-9][0-1]\\d{8}$",
                    IsActive = true
                },
                new UtilityProvider
                {
                    Id = 6,
                    Name = "GLO Data",
                    Code = "GLO",
                    Type = BillType.Data,
                    Description = "GLO Mobile Data",
                    ApiEndpoint = "https://api.glo.com/v1",
                    RequiresAccountValidation = false,
                    AccountNumberFormat = "^0[7-9][0-1]\\d{8}$",
                    IsActive = true
                }
            );
        }






    }
    
}
