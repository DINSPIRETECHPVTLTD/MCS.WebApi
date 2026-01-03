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
        public DbSet<OrganizationUser> OrganizationUsers { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<BranchUser> BranchUsers { get; set; }
        public DbSet<Center> Centers { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Guardian> Guardians { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure soft delete query filters
            modelBuilder.Entity<Organization>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<OrganizationUser>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<Branch>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<BranchUser>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Center>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Member>().HasQueryFilter(m => !m.IsDeleted);

            // Configure relationships
            modelBuilder.Entity<OrganizationUser>()
                .HasOne(o => o.Organization)
                .WithMany(org => org.OrganizationUsers)
                .HasForeignKey(o => o.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Organization)
                .WithMany(org => org.Branches)
                .HasForeignKey(b => b.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BranchUser>()
                .HasOne(bu => bu.Branch)
                .WithMany(b => b.BranchUsers)
                .HasForeignKey(bu => bu.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Center>()
                .HasOne(c => c.Branch)
                .WithMany(b => b.Centers)
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Branch)
                .WithMany(b => b.Members)
                .HasForeignKey(m => m.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Center)
                .WithMany(c => c.Members)
                .HasForeignKey(m => m.CenterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Guardian>()
                .HasOne(g => g.Member)
                .WithMany(m => m.Guardians)
                .HasForeignKey(g => g.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            modelBuilder.Entity<OrganizationUser>()
                .HasIndex(o => o.Email)
                .IsUnique();

            modelBuilder.Entity<BranchUser>()
                .HasIndex(b => b.Email)
                .IsUnique();
        }

        public override int SaveChanges()
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateSoftDeleteStatuses()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Organization || 
                    entry.Entity is OrganizationUser || 
                    entry.Entity is Branch || 
                    entry.Entity is BranchUser || 
                    entry.Entity is Center || 
                    entry.Entity is Member)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Property("IsDeleted").CurrentValue = false;
                            var createdDateValue = entry.Property("CreatedDate").CurrentValue;
                            if (createdDateValue == null || 
                                (createdDateValue is DateTime date && date == default(DateTime)))
                            {
                                entry.Property("CreatedDate").CurrentValue = DateTime.UtcNow;
                            }
                            break;
                        case EntityState.Modified:
                            entry.Property("IsDeleted").IsModified = false;
                            break;
                    }
                }
            }
        }
    }
}

