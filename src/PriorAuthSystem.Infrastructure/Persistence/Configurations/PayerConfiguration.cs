using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PriorAuthSystem.Domain.Entities;

namespace PriorAuthSystem.Infrastructure.Persistence.Configurations;

public class PayerConfiguration : IEntityTypeConfiguration<Payer>
{
    public void Configure(EntityTypeBuilder<Payer> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PayerName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.PayerId).HasMaxLength(50).IsRequired();
        builder.Property(p => p.StandardResponseDays).IsRequired();

        builder.HasIndex(p => p.PayerId).IsUnique();

        builder.OwnsOne(p => p.ContactInfo, ci =>
        {
            ci.Property(c => c.Phone).HasMaxLength(20).IsRequired().HasColumnName("Phone");
            ci.Property(c => c.Email).HasMaxLength(200).HasColumnName("Email");
            ci.Property(c => c.FaxNumber).HasMaxLength(20).HasColumnName("FaxNumber");
        });

        builder.Ignore(p => p.DomainEvents);
    }
}
