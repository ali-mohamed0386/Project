using JobApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobApplication.Infrastructure.Persistence.Configurations
{
    public class RecruiterConfiguration : IEntityTypeConfiguration<Recruiter>
    {
        public void Configure(EntityTypeBuilder<Recruiter> builder)
        {
            builder.ToTable("Recruiters");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(r => r.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(r => r.PasswordHash)
                   .IsRequired();

            builder.HasIndex(r => r.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_Recruiters_Email");
        }
    }
}
