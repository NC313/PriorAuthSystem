# Domain Glossary

## Prior Authorization (PA)

A prior authorization is a formal approval that a healthcare provider must obtain from a payer before delivering a specific medical service, procedure, or medication. The payer reviews clinical documentation to determine whether the requested service is medically necessary and covered under the patient's plan. Without prior authorization, the provider risks non-reimbursement.

## Payer

A payer is an insurance company or health plan responsible for reimbursing healthcare providers for covered services. Examples include Blue Cross Blue Shield, Aetna, and UnitedHealthcare. In this system, payers are the organizations that receive, review, and ultimately approve or deny prior authorization requests.

## Provider

A provider is the physician, clinic, or healthcare facility that requests prior authorization on behalf of a patient. Providers submit clinical documentation supporting the medical necessity of the requested service and track the status of their authorization requests through the system.

## Patient / Member

A patient (also referred to as a member in the insurance context) is the individual receiving medical care. Each patient is enrolled with a payer under a specific health plan, and their coverage determines which services require prior authorization.

## Status Lifecycle

Every prior authorization request follows a defined status lifecycle: **Draft** (created but not yet submitted), **Submitted** (sent to payer for review), **UnderReview** (actively being evaluated by a reviewer), **PendingInfo** (reviewer has requested additional clinical documentation), **Approved** (authorization granted), **Denied** (authorization refused), or **Cancelled** (withdrawn by the provider). All transitions between statuses are enforced at the domain layer to prevent invalid state changes.

## FHIR R4

HL7 Fast Healthcare Interoperability Resources (FHIR) is a standard for exchanging healthcare information electronically. R4 is the current stable release of the specification. This system exposes FHIR R4-compliant endpoints for Patient, Practitioner, Coverage, and Claim resources to support interoperability with other healthcare systems.

## Clinical Notes

Clinical notes are free-text supporting documentation that providers attach to a prior authorization request. These notes describe the patient's condition, the medical necessity of the requested service, and any relevant clinical history that supports the authorization decision.

## Aggregate Root

An aggregate root is a Domain-Driven Design (DDD) pattern where a single entity serves as the entry point for a cluster of related objects. In this system, `PriorAuthorizationRequest` is the aggregate root—it owns all status transitions and domain events, ensuring that business rules governing the authorization lifecycle are enforced consistently within the domain layer rather than scattered across application services.
