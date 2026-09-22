using JobApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplication.Infrastructure.Persistence.Configurations
{
    public class JobCandidateApplicationConfiguration : IEntityTypeConfiguration<JobCandidateApplication>
    {
        public void Configure(EntityTypeBuilder<JobCandidateApplication> builder)
        {
            // This is not a performance tweak — it is the real protection
            // against duplicate applications.
            //
            // Relying on an application-level check alone:
            //   Request A: SELECT -> no application found  OK
            //   Request B: SELECT -> no application found  OK   <- race condition
            //   Request A: INSERT                          OK
            //   Request B: INSERT                          OK   <- duplicate created!
            //
            // The unique index is the only guarantee that cannot be raced.
            builder.HasIndex(a => new { a.CandidateId, a.JobId })
                   .IsUnique()
                   .HasDatabaseName("IX_JobCandidateApplications_CandidateId_JobId");
        }
    }
}
