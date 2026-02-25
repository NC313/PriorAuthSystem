using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PriorAuthSystem.Domain.Entities;

namespace PriorAuthSystem.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.MemberId).HasMaxLength(50).IsRequired();
        builder.Property(p => p.InsurancePlanId).HasMaxLength(50);
        builder.Property(p => p.DateOfBirth).IsRequired();

        builder.OwnsOne(p => p.ContactInfo, ci =>
        {
            ci.Property(c => c.Phone).HasMaxLength(20).IsRequired().HasColumnName("Phone");
            ci.Property(c => c.Email).HasMaxLength(200).HasColumnName("Email");
            ci.Property(c => c.FaxNumber).HasMaxLength(20).HasColumnName("FaxNumber");
        });

        builder.Ignore(p => p.FullName);
        builder.Ignore(p => p.Age);
        builder.Ignore(p => p.DomainEvents);
    }
}
