using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Enums;

namespace PriorAuthSystem.Infrastructure.Persistence.Configurations;

public class StatusTransitionConfiguration : IEntityTypeConfiguration<StatusTransition>
{
    public void Configure(EntityTypeBuilder<StatusTransition> builder)
    {
        builder.HasKey(st => st.Id);

        builder.Property(st => st.PriorAuthorizationRequestId).IsRequired();

        builder.Property(st => st.FromStatus)
            .HasConversion(
                s => s.ToString(),
                s => Enum.Parse<PriorAuthStatus>(s))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(st => st.ToStatus)
            .HasConversion(
                s => s.ToString(),
                s => Enum.Parse<PriorAuthStatus>(s))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(st => st.TransitionedBy).HasMaxLength(200).IsRequired();
        builder.Property(st => st.Notes).HasMaxLength(2000);
        builder.Property(st => st.TransitionedAt).IsRequired();

        builder.HasIndex(st => st.PriorAuthorizationRequestId);

        builder.Ignore(st => st.DomainEvents);
    }
}
