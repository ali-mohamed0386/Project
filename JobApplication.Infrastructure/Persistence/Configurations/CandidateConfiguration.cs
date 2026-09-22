using JobApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplication.Infrastructure.Persistence.Configurations
{
    public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
    {
        public void Configure(EntityTypeBuilder<Candidate> builder)
        {
            builder.Property(c => c.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(c => c.PasswordHash)
                   .IsRequired();

            // One email address = one account. Duplicates are rejected at the
            // database level.
            builder.HasIndex(c => c.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_Candidates_Email");
        }
    }
}
