using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Enums;

namespace PriorAuthSystem.Infrastructure.Persistence.Configurations;

public class PriorAuthorizationRequestConfiguration : IEntityTypeConfiguration<PriorAuthorizationRequest>
{
    public void Configure(EntityTypeBuilder<PriorAuthorizationRequest> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Status)
            .HasConversion(
                s => s.ToString(),
                s => Enum.Parse<PriorAuthStatus>(s))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.RequiredResponseBy).IsRequired();

        builder.HasOne(p => p.Patient)
            .WithMany()
            .IsRequired();

        builder.HasOne(p => p.Provider)
            .WithMany()
            .IsRequired();

        builder.HasOne(p => p.Payer)
            .WithMany()
            .IsRequired();

        builder.OwnsOne(p => p.IcdCode, icd =>
        {
            icd.Property(i => i.Code).HasMaxLength(7).IsRequired().HasColumnName("IcdCode");
            icd.Property(i => i.Description).HasMaxLength(500).HasColumnName("IcdDescription");
        });

        builder.OwnsOne(p => p.CptCode, cpt =>
        {
            cpt.Property(c => c.Code).HasMaxLength(5).IsRequired().HasColumnName("CptCode");
            cpt.Property(c => c.Description).HasMaxLength(500).HasColumnName("CptDescription");
            cpt.Property(c => c.RequiresPriorAuth).HasColumnName("CptRequiresPriorAuth");
        });

        builder.OwnsOne(p => p.ClinicalJustification, cj =>
        {
            cj.Property(c => c.Notes).HasMaxLength(4000).IsRequired().HasColumnName("ClinicalNotes");
            cj.Property(c => c.DocumentedBy).HasMaxLength(200).IsRequired().HasColumnName("ClinicalDocumentedBy");
            cj.Property(c => c.SupportingDocumentPath).HasMaxLength(500).HasColumnName("ClinicalSupportingDocumentPath");
            cj.Property(c => c.DocumentedAt).HasColumnName("ClinicalDocumentedAt");
        });

        builder.HasMany(p => p.StatusTransitions)
            .WithOne()
            .HasForeignKey(st => st.PriorAuthorizationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(PriorAuthorizationRequest.StatusTransitions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(p => p.DomainEvents);
    }
}
