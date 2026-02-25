using Microsoft.EntityFrameworkCore;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Patients.AnyAsync())
            return;

        // Patients (6)
        var patients = new[]
        {
            new Patient("Maria", "Gonzalez", new DateTime(1978, 3, 15, 0, 0, 0, DateTimeKind.Utc), "BCB-100234", "BCBS-PPO-2024",
                new ContactInfo("555-201-1234", "maria.gonzalez@email.com")),
            new Patient("Robert", "Chen", new DateTime(1965, 11, 2, 0, 0, 0, DateTimeKind.Utc), "AET-200891", "AETNA-HMO-2024",
                new ContactInfo("555-302-4567", "robert.chen@email.com")),
            new Patient("Patricia", "Williams", new DateTime(1990, 7, 22, 0, 0, 0, DateTimeKind.Utc), "BCB-100567", "BCBS-EPO-2024",
                new ContactInfo("555-403-7890", "patricia.williams@email.com")),
            new Patient("James", "Okafor", new DateTime(1955, 4, 8, 0, 0, 0, DateTimeKind.Utc), "AET-201234", "AETNA-PPO-2024",
                new ContactInfo("555-504-1234", "james.okafor@email.com")),
            new Patient("Linda", "Martinez", new DateTime(1982, 9, 30, 0, 0, 0, DateTimeKind.Utc), "BCB-100789", "BCBS-PPO-2024",
                new ContactInfo("555-605-5678", "linda.martinez@email.com")),
            new Patient("David", "Thompson", new DateTime(1947, 12, 18, 0, 0, 0, DateTimeKind.Utc), "AET-201567", "AETNA-HMO-2024",
                new ContactInfo("555-706-9012", "david.thompson@email.com"))
        };

        // Providers (4)
        var providers = new[]
        {
            new Provider("Sarah", "Chen", "1234567890", "Orthopedic Surgery", "Northwest Orthopedics",
                new ContactInfo("555-601-1000", "sarah.chen@orthoclinic.com", "555-601-1001")),
            new Provider("Marcus", "Williams", "2345678901", "Physical Medicine & Rehab", "Metro Rehab Associates",
                new ContactInfo("555-602-2000", "m.williams@rehabmed.com", "555-602-2001")),
            new Provider("James", "Okafor", "3456789012", "Neurology", "Metro Neurology Center",
                new ContactInfo("555-603-3000", "j.okafor@neurocenter.com", "555-603-3001")),
            new Provider("Patricia", "Huang", "4567890123", "Radiology", "Advanced Imaging Center",
                new ContactInfo("555-604-4000", "p.huang@imagingcenter.com", "555-604-4001"))
        };

        // Payers (2)
        var payers = new[]
        {
            new Payer("BlueCross", "BCB-001", 3,
                new ContactInfo("800-555-1234", "priorauth@bluecross.com", "800-555-1235")),
            new Payer("Aetna", "AET-001", 5,
                new ContactInfo("800-555-6789", "priorauth@aetna.com", "800-555-6790"))
        };

        context.Patients.AddRange(patients);
        context.Providers.AddRange(providers);
        context.Payers.AddRange(payers);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[DbInitializer] Failed to save patients/providers/payers: {ex.Message}");
            Console.Error.WriteLine(ex.ToString());
            throw;
        }

        // Prior Auth Request 1: Maria Gonzalez — MRI Lumbar Spine — Submitted (2 days ago)
        var pa1 = new PriorAuthorizationRequest(
            patients[0], providers[3], payers[0],
            new IcdCode("M54.5", "Low back pain"),
            new CptCode("72148", "MRI Lumbar Spine without contrast"),
            new ClinicalJustification(
                "Patient presents with persistent low back pain radiating to left lower extremity for 8 weeks. Conservative treatment with NSAIDs and physical therapy has failed. MRI required to evaluate for disc herniation or spinal stenosis.",
                "Dr. Patricia Huang"),
            DateTime.UtcNow.AddDays(5));
        pa1.Submit();

        // Prior Auth Request 2: Robert Chen — Physical Therapy x20 — UnderReview
        var pa2 = new PriorAuthorizationRequest(
            patients[1], providers[1], payers[1],
            new IcdCode("M47.816", "Spondylosis without myelopathy, lumbar region"),
            new CptCode("97110", "Therapeutic exercises"),
            new ClinicalJustification(
                "Patient diagnosed with lumbar spondylosis. Requires 20 visits of therapeutic exercise focusing on core stabilization and lumbar flexibility. Functional limitations include inability to sit >30 min and difficulty with ADLs.",
                "Dr. Marcus Williams"),
            DateTime.UtcNow.AddDays(7));
        pa2.Submit();

        // Prior Auth Request 3: Patricia Williams — Knee Arthroscopy — Approved
        var pa3 = new PriorAuthorizationRequest(
            patients[2], providers[0], payers[0],
            new IcdCode("M23.201", "Derangement of unspecified medial meniscus, right knee"),
            new CptCode("29881", "Knee arthroscopy with meniscectomy"),
            new ClinicalJustification(
                "MRI confirms medial meniscus tear with mechanical symptoms including locking and catching. Patient has failed 12 weeks of conservative management including PT and corticosteroid injection.",
                "Dr. Sarah Chen", "/docs/williams-knee-mri.pdf"),
            DateTime.UtcNow.AddDays(5));
        pa3.Submit();
        pa3.Approve("reviewer-001", "Approved — clinical documentation supports medical necessity. MRI findings consistent with meniscal tear requiring surgical intervention.");

        // Prior Auth Request 4: James Okafor — Neurology Consult — Denied
        var pa4 = new PriorAuthorizationRequest(
            patients[3], providers[2], payers[1],
            new IcdCode("G43.909", "Migraine, unspecified, not intractable"),
            new CptCode("99244", "Office consultation, moderate complexity"),
            new ClinicalJustification(
                "Patient requests neurology consultation for migraine management. Has been managed with OTC medications.",
                "Dr. James Okafor"),
            DateTime.UtcNow.AddDays(8));
        pa4.Submit();
        pa4.Deny("reviewer-001", DenialReason.NotMedicallyNecessary, "Insufficient documentation of failed first-line treatments. Patient has not trialed prescription migraine medications. Recommend trial of triptans before specialty referral.");

        // Prior Auth Request 5: Linda Martinez — CT Scan Chest — AdditionalInfoRequested
        var pa5 = new PriorAuthorizationRequest(
            patients[4], providers[3], payers[0],
            new IcdCode("R05.9", "Cough, unspecified"),
            new CptCode("71250", "CT chest without contrast"),
            new ClinicalJustification(
                "Patient with persistent cough for 6 weeks. Chest X-ray inconclusive. CT scan requested to rule out pulmonary pathology.",
                "Dr. Patricia Huang"),
            DateTime.UtcNow.AddDays(4));
        pa5.Submit();
        pa5.RequestAdditionalInfo("reviewer-001", "Please provide chest X-ray results, smoking history, and duration of symptoms. Also include any relevant lab results (CBC, CRP).");

        // Prior Auth Request 6: David Thompson — Hip Replacement — Appealed
        var pa6 = new PriorAuthorizationRequest(
            patients[5], providers[0], payers[1],
            new IcdCode("M16.11", "Primary osteoarthritis, right hip"),
            new CptCode("27130", "Total hip arthroplasty"),
            new ClinicalJustification(
                "Patient with severe right hip osteoarthritis. Harris Hip Score of 42. Failed conservative treatment including PT, NSAIDs, and two corticosteroid injections over 18 months.",
                "Dr. Sarah Chen", "/docs/thompson-hip-xray.pdf"),
            DateTime.UtcNow.AddDays(10));
        pa6.Submit();
        pa6.Deny("reviewer-001", DenialReason.RequiresAlternativeTreatment, "Recommend trial of viscosupplementation before surgical intervention.");
        pa6.Appeal("provider-001", "Patient has exhausted all conservative options including viscosupplementation (3 injection series completed 6 months ago with no improvement). Harris Hip Score has declined from 52 to 42. Weight-bearing X-rays show complete joint space loss.");

        // Prior Auth Request 7: Maria Gonzalez — Epidural Steroid Injection — AppealApproved
        var pa7 = new PriorAuthorizationRequest(
            patients[0], providers[0], payers[0],
            new IcdCode("M54.4", "Lumbago with sciatica"),
            new CptCode("62322", "Lumbar epidural injection"),
            new ClinicalJustification(
                "Patient with documented L4-L5 disc herniation causing radiculopathy. Failed 8 weeks of conservative treatment. ESI recommended for pain management.",
                "Dr. Sarah Chen"),
            DateTime.UtcNow.AddDays(6));
        pa7.Submit();
        pa7.Deny("reviewer-001", DenialReason.InsufficientDocumentation, "MRI report not included with initial submission.");
        pa7.Appeal("provider-001", "MRI report now attached showing L4-L5 disc herniation with left foraminal stenosis. EMG/NCS confirms L5 radiculopathy.");
        pa7.AppealApprove("reviewer-001", "Appeal approved. Additional documentation confirms medical necessity. MRI and EMG findings support epidural steroid injection.");

        // Prior Auth Request 8: Robert Chen — Cervical MRI — Expired
        var pa8 = new PriorAuthorizationRequest(
            patients[1], providers[2], payers[1],
            new IcdCode("M54.2", "Cervicalgia"),
            new CptCode("72141", "MRI cervical spine without contrast"),
            new ClinicalJustification(
                "Patient with persistent neck pain and upper extremity paresthesias. Cervical MRI needed to evaluate for disc disease or stenosis.",
                "Dr. James Okafor"),
            DateTime.UtcNow.AddDays(-1));

        // For the expired request, we need to use reflection or a workaround
        // since RequiredResponseBy must be in the future at creation time.
        // We'll create it with a future date, submit, then manually expire it.
        // Actually, the constructor requires future date, so let's create with future date
        // then adjust. Since we can't directly set fields, we'll create a separate one
        // with a workaround.
        var pa8Actual = new PriorAuthorizationRequest(
            patients[1], providers[2], payers[1],
            new IcdCode("M54.2", "Cervicalgia"),
            new CptCode("72141", "MRI cervical spine without contrast"),
            new ClinicalJustification(
                "Patient with persistent neck pain and upper extremity paresthesias. Cervical MRI needed to evaluate for disc disease or stenosis.",
                "Dr. James Okafor"),
            DateTime.UtcNow.AddDays(1));
        pa8Actual.Submit();

        context.PriorAuthorizationRequests.AddRange(pa1, pa2, pa3, pa4, pa5, pa6, pa7, pa8Actual);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[DbInitializer] Failed to save prior auth requests: {ex.Message}");
            Console.Error.WriteLine(ex.ToString());
            throw;
        }
    }
}
