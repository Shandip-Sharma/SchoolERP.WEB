using Application.Common.Interface;
using Domain.Entities.Security;
using Domain.Entities.Student;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<StudentBasicInfo> StudentBasicInfos { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Domain.Entities.Common.BaseEntity auditEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (auditEntity.CreatedOn == default)
                        {
                            auditEntity.CreatedOn = DateTime.UtcNow;
                        }
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditEntity.ModifiedOn = DateTime.UtcNow;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StudentBasicInfo>(entity =>
            {
                entity.ToTable("StudentBasicInfos");
                
                entity.HasOne(s => s.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(s => s.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.ModifiedByUser)
                    .WithMany()
                    .HasForeignKey(s => s.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
