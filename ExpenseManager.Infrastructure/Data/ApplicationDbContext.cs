using ExpenseManager.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletActivity> WalletActivities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Decimal precision for SQLite (though SQLite doesn't strictly enforce it, EF Core likes it)
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("TEXT"); // SQLite stores decimals as TEXT or REAL usually, but EF core can map it
            }

            // Relationship configuration
            builder.Entity<Expense>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Expense>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WalletActivity>()
                .HasOne(wa => wa.User)
                .WithMany()
                .HasForeignKey(wa => wa.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexing for CompanyId to support multi-tenancy performance
            builder.Entity<ApplicationUser>().HasIndex(u => u.CompanyId);
            builder.Entity<Expense>().HasIndex(e => e.CompanyId);
            builder.Entity<Category>().HasIndex(c => c.CompanyId);
            builder.Entity<Wallet>().HasIndex(w => w.CompanyId);
            builder.Entity<WalletActivity>().HasIndex(wa => wa.CompanyId);
        }
    }
}
