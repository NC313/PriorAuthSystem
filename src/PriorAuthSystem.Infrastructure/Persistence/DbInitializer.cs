using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Patients.AnyAsync())
            return;

        // Patients
        var patients = new[]
        {
            new Patient("Maria", "Garcia", new DateTime(1985, 3, 14), "MBR-100234", "BCBS-PPO-2024",
                new ContactInfo("555-201-1234", "maria.garcia@email.com", "555-201-1235")),
            new Patient("James", "Thompson", new DateTime(1972, 8, 22), "MBR-100567", "AETNA-HMO-2024",
                new ContactInfo("555-302-4567", "james.thompson@email.com")),
            new Patient("Aisha", "Patel", new DateTime(1990, 11, 5), "MBR-100891", "BCBS-EPO-2024",
                new ContactInfo("555-403-7890", "aisha.patel@email.com", "555-403-7891"))
        };

        // Providers
        var providers = new[]
        {
            new Provider("Sarah", "Chen", "1234567890", "Orthopedic Surgery", "Northwest Orthopedics",
                new ContactInfo("555-601-1000", "office@nwortho.com", "555-601-1001")),
            new Provider("Michael", "Rodriguez", "2345678901", "Neurology", "Metro Neurology Associates",
                new ContactInfo("555-602-2000", "office@metroneuro.com", "555-602-2001")),
            new Provider("Emily", "Watson", "3456789012", "Physical Medicine & Rehabilitation", "City Rehab Center",
                new ContactInfo("555-603-3000", "office@cityrehab.com", "555-603-3001"))
        };

        // Payers
        var payers = new[]
        {
            new Payer("BlueCross BlueShield", "BCBS-001", 3,
                new ContactInfo("800-555-1234", "priorauth@bcbs.com", "800-555-1235")),
            new Payer("Aetna", "AETNA-001", 3,
                new ContactInfo("800-555-6789", "priorauth@aetna.com", "800-555-6790"))
        };

        context.Patients.AddRange(patients);
        context.Providers.AddRange(providers);
        context.Payers.AddRange(payers);

        // Prior Auth Requests with various statuses
        var pa1 = new PriorAuthorizationRequest(
            patients[0], providers[0], payers[0],
            new IcdCode("M17.11", "Primary osteoarthritis, right knee"),
            new CptCode("27447", "Total knee replacement"),
            new ClinicalJustification(
                "Patient has failed 6 months of conservative treatment including physical therapy and NSAIDs. X-rays show bone-on-bone changes.",
                "Dr. Sarah Chen", "/docs/garcia-knee-xray.pdf"),
            DateTime.UtcNow.AddDays(3));
        pa1.Submit();

        var pa2 = new PriorAuthorizationRequest(
            patients[1], providers[1], payers[1],
            new IcdCode("G43.909", "Migraine, unspecified, not intractable"),
            new CptCode("70553", "MRI brain with and without contrast"),
            new ClinicalJustification(
                "Patient presents with new-onset severe migraines with visual aura. MRI required to rule out intracranial pathology.",
                "Dr. Michael Rodriguez"),
            DateTime.UtcNow.AddDays(3));
        pa2.Submit();

        var pa3 = new PriorAuthorizationRequest(
            patients[2], providers[2], payers[0],
            new IcdCode("M54.5", "Low back pain"),
            new CptCode("97110", "Therapeutic exercises"),
            new ClinicalJustification(
                "Patient requires 12 sessions of physical therapy for chronic low back pain following lumbar disc herniation.",
                "Dr. Emily Watson"),
            DateTime.UtcNow.AddDays(3));
        pa3.Submit();

        var pa4 = new PriorAuthorizationRequest(
            patients[0], providers[1], payers[0],
            new IcdCode("G47.33", "Obstructive sleep apnea"),
            new CptCode("95811", "Polysomnography with CPAP titration"),
            new ClinicalJustification(
                "Patient with BMI 34 and Epworth Sleepiness Scale score of 16. Overnight sleep study required.",
                "Dr. Michael Rodriguez"),
            DateTime.UtcNow.AddDays(3));
        pa4.Submit();

        var pa5 = new PriorAuthorizationRequest(
            patients[1], providers[0], payers[1],
            new IcdCode("M75.110", "Incomplete rotator cuff tear of right shoulder"),
            new CptCode("29827", "Arthroscopic rotator cuff repair"),
            new ClinicalJustification(
                "MRI confirms partial-thickness rotator cuff tear. Patient has failed 3 months of conservative treatment. Surgical repair recommended.",
                "Dr. Sarah Chen", "/docs/thompson-shoulder-mri.pdf"),
            DateTime.UtcNow.AddDays(3));
        pa5.Submit();

        context.PriorAuthorizationRequests.AddRange(pa1, pa2, pa3, pa4, pa5);
        await context.SaveChangesAsync();
    }
}
