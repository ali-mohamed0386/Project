using JobApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplication.Infrastructure.Persistence.Configurations
{
    public class JobConfiguration : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            // The only new relationship: every job has exactly one owner (Recruiter).
            builder.HasOne(j => j.Recruiter)
                   .WithMany()
                   .HasForeignKey(j => j.RecruiterId)
                   // Restrict rather than Cascade — deleting a recruiter must not
                   // take all of their jobs (and every application submitted to
                   // them) down with it.
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(j => j.RecruiterId);
        }
    }
}
