using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PriorAuthSystem.Domain.Entities;

namespace PriorAuthSystem.Infrastructure.Persistence.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.NPI).HasMaxLength(10).IsRequired();
        builder.Property(p => p.Specialty).HasMaxLength(100).IsRequired();
        builder.Property(p => p.OrganizationName).HasMaxLength(200);

        builder.HasIndex(p => p.NPI).IsUnique();

        builder.OwnsOne(p => p.ContactInfo, ci =>
        {
            ci.Property(c => c.Phone).HasMaxLength(20).IsRequired().HasColumnName("Phone");
            ci.Property(c => c.Email).HasMaxLength(200).HasColumnName("Email");
            ci.Property(c => c.FaxNumber).HasMaxLength(20).HasColumnName("FaxNumber");
        });

        builder.Ignore(p => p.FullName);
        builder.Ignore(p => p.DomainEvents);
    }
}
