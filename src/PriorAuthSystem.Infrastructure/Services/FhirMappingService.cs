using Hl7.Fhir.Model;
using PriorAuthSystem.Domain.Entities;
using FhirPatient = Hl7.Fhir.Model.Patient;
using DomainPatient = PriorAuthSystem.Domain.Entities.Patient;

namespace PriorAuthSystem.Infrastructure.Services;

public class FhirMappingService
{
    public FhirPatient MapToFhirPatient(DomainPatient patient)
    {
        var fhirPatient = new FhirPatient
        {
            Id = patient.Id.ToString(),
            Name =
            {
                new HumanName
                {
                    Family = patient.LastName,
                    Given = new[] { patient.FirstName },
                    Use = HumanName.NameUse.Official
                }
            },
            BirthDate = patient.DateOfBirth.ToString("yyyy-MM-dd"),
            Identifier =
            {
                new Identifier
                {
                    System = "urn:oid:member-id",
                    Value = patient.MemberId
                }
            },
            Telecom =
            {
                new ContactPoint
                {
                    System = ContactPoint.ContactPointSystem.Phone,
                    Value = patient.ContactInfo.Phone,
                    Use = ContactPoint.ContactPointUse.Work
                }
            }
        };

        if (!string.IsNullOrEmpty(patient.ContactInfo.Email))
        {
            fhirPatient.Telecom.Add(new ContactPoint
            {
                System = ContactPoint.ContactPointSystem.Email,
                Value = patient.ContactInfo.Email,
                Use = ContactPoint.ContactPointUse.Work
            });
        }

        if (!string.IsNullOrEmpty(patient.ContactInfo.FaxNumber))
        {
            fhirPatient.Telecom.Add(new ContactPoint
            {
                System = ContactPoint.ContactPointSystem.Fax,
                Value = patient.ContactInfo.FaxNumber,
                Use = ContactPoint.ContactPointUse.Work
            });
        }

        if (!string.IsNullOrEmpty(patient.InsurancePlanId))
        {
            fhirPatient.Identifier.Add(new Identifier
            {
                System = "urn:oid:insurance-plan-id",
                Value = patient.InsurancePlanId
            });
        }

        return fhirPatient;
    }

    public Claim MapToFhirClaim(PriorAuthorizationRequest priorAuth)
    {
        var claim = new Claim
        {
            Id = priorAuth.Id.ToString(),
            Status = MapToFhirClaimStatus(priorAuth.Status),
            Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/claim-type", "professional"),
            Use = ClaimUseCode.Preauthorization,
            Created = priorAuth.CreatedAt.ToString("yyyy-MM-dd"),
            Patient = new ResourceReference($"Patient/{priorAuth.Patient.Id}"),
            Provider = new ResourceReference($"Practitioner/{priorAuth.Provider.Id}"),
            Insurer = new ResourceReference($"Organization/{priorAuth.Payer.Id}"),
            Priority = new CodeableConcept("http://terminology.hl7.org/CodeSystem/processpriority", "normal"),
            Diagnosis =
            {
                new Claim.DiagnosisComponent
                {
                    Sequence = 1,
                    Diagnosis = new CodeableConcept(
                        "http://hl7.org/fhir/sid/icd-10-cm",
                        priorAuth.IcdCode.Code,
                        priorAuth.IcdCode.Description)
                }
            },
            Item =
            {
                new Claim.ItemComponent
                {
                    Sequence = 1,
                    ProductOrService = new CodeableConcept(
                        "http://www.ama-assn.org/go/cpt",
                        priorAuth.CptCode.Code,
                        priorAuth.CptCode.Description)
                }
            },
            SupportingInfo =
            {
                new Claim.SupportingInformationComponent
                {
                    Sequence = 1,
                    Category = new CodeableConcept(
                        "http://terminology.hl7.org/CodeSystem/claiminformationcategory",
                        "info"),
                    Value = new FhirString(priorAuth.ClinicalJustification.Notes)
                }
            }
        };

        return claim;
    }

    private static FinancialResourceStatusCodes MapToFhirClaimStatus(Domain.Enums.PriorAuthStatus status) =>
        status switch
        {
            Domain.Enums.PriorAuthStatus.Draft => FinancialResourceStatusCodes.Draft,
            Domain.Enums.PriorAuthStatus.Canceled => FinancialResourceStatusCodes.Cancelled,
            _ => FinancialResourceStatusCodes.Active
        };
}
