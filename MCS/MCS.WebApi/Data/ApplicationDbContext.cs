using System;
using Microsoft.EntityFrameworkCore;
using MCS.WebApi.Models;

namespace MCS.WebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Center> Centers { get; set; }
        public DbSet<POC> POCs { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanPayment> LoanPayments { get; set; }
        public DbSet<MasterLookup> MasterLookups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MasterLookup>(entity =>
            {
                entity.ToTable("MasterLookups");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.LookupKey)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.LookupCode)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.LookupValue)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.NumericValue)
                      .HasPrecision(18, 2);

                entity.HasIndex(e => new { e.LookupKey, e.LookupCode })
                      .IsUnique();
            });

            // Configure soft delete query filters
            modelBuilder.Entity<Organization>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Branch>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Center>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<POC>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Member>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Loan>().HasQueryFilter(l => !l.IsDeleted);
            modelBuilder.Entity<LoanPayment>().HasQueryFilter(lp => !lp.IsDeleted);

            // Configure User enum conversions (database stores as string, C# uses enum)
            // Role: 'Owner', 'BranchAdmin', 'Staff'
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(
                    v => v.ToString(),           // Convert enum to string for database
                    v => (UserRole)Enum.Parse(typeof(UserRole), v)); // Convert string to enum from database

            // Level: 'Org', 'Branch'
            modelBuilder.Entity<User>()
                .Property(u => u.Level)
                .HasConversion(
                    v => v.ToString(),           // Convert enum to string for database
                    v => (UserLevel)Enum.Parse(typeof(UserLevel), v)); // Convert string to enum from database

            // Configure User relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Organization)
                .WithMany(org => org.Users)
                .HasForeignKey(u => u.OrgId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing relationships for CreatedBy/ModifiedBy
            modelBuilder.Entity<User>()
                .HasOne(u => u.CreatedByUser)
                .WithMany()
                .HasForeignKey(u => u.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.ModifiedByUser)
                .WithMany()
                .HasForeignKey(u => u.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Branch relationships
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Organization)
                .WithMany(org => org.Branches)
                .HasForeignKey(b => b.OrgId)
                .OnDelete(DeleteBehavior.Restrict);

           modelBuilder.Entity<Branch>()
                .HasOne(b => b.CreatedByUser)
                .WithMany()
                .HasForeignKey(b => b.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.ModifiedByUser)
                .WithMany()
                .HasForeignKey(b => b.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Organization relationships
            modelBuilder.Entity<Organization>()
                .HasOne(o => o.CreatedByUser)
                .WithMany()
                .HasForeignKey(o => o.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Organization>()
                .HasOne(o => o.ModifiedByUser)
                .WithMany()
                .HasForeignKey(o => o.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Center relationships
            modelBuilder.Entity<Center>()
                 .HasOne(c => c.Branch)
                 .WithMany(b => b.Centers)
                 .HasForeignKey(c => c.BranchId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Center>()
                .HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Center>()
                .HasOne(c => c.ModifiedByUser)
                .WithMany()
                .HasForeignKey(c => c.ModifiedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure POC relationships
            modelBuilder.Entity<POC>()
                .HasOne(p => p.Center)
                .WithMany(c => c.POCs)
                .HasForeignKey(p => p.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<POC>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<POC>()
                .HasOne(p => p.ModifiedByUser)
                .WithMany()
                .HasForeignKey(p => p.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Member relationships
            modelBuilder.Entity<Member>()
                .HasOne(m => m.Center)
                .WithMany(c => c.Members)
                .HasForeignKey(m => m.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.POC)
                .WithMany(p => p.Members)
                .HasForeignKey(m => m.POCId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.CreatedByUser)
                .WithMany()
                .HasForeignKey(m => m.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.ModifiedByUser)
                .WithMany()
                .HasForeignKey(m => m.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Loan relationships
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Member)
                .WithMany()
                .HasForeignKey(l => l.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.CreatedByUser)
                .WithMany()
                .HasForeignKey(l => l.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.ModifiedByUser)
                .WithMany()
                .HasForeignKey(l => l.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure LoanPayment relationships
            modelBuilder.Entity<LoanPayment>()
                .HasOne(lp => lp.Loan)
                .WithMany(l => l.LoanPayments)
                .HasForeignKey(lp => lp.LoanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanPayment>()
                .HasOne(lp => lp.CreatedByUser)
                .WithMany()
                .HasForeignKey(lp => lp.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanPayment>()
                .HasOne(lp => lp.ModifiedByUser)
                .WithMany()
                .HasForeignKey(lp => lp.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Organization || 
                    entry.Entity is User || 
                    entry.Entity is Branch || 
                    entry.Entity is Center || 
                    entry.Entity is POC || 
                    entry.Entity is Member ||
                    entry.Entity is Loan ||
                    entry.Entity is LoanPayment)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Property("IsDeleted").CurrentValue = false;
                            var createdAtValue = entry.Property("CreatedAt").CurrentValue;
                            if (createdAtValue == null || 
                                (createdAtValue is DateTime date && date == default(DateTime)))
                            {
                                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                            }
                            break;
                        case EntityState.Modified:
                            // Allow soft-delete (IsDeleted = true) to be persisted; only ignore IsDeleted when false (normal update)
                            if (entry.Property("IsDeleted").CurrentValue is bool isDeleted && !isDeleted)
                            {
                                entry.Property("IsDeleted").IsModified = false;
                            }
                            entry.Property("ModifiedAt").CurrentValue = DateTime.UtcNow;
                            break;
                    }
                }
            }
        }
    }
}

