# Architecture Decision Records

## ADR-1: Clean Architecture

**Status:** Accepted

**Context:** The system needs clear separation of concerns, testability without infrastructure dependencies, and the ability to evolve the UI and persistence layers independently of business logic.

**Decision:** Adopt four-layer Clean Architecture with Domain, Application, Infrastructure, and API layers. The dependency rule is strictly enforced—inner layers (Domain, Application) have no references to outer layers or framework-specific packages. Infrastructure implements interfaces defined in Domain.

**Consequences:** Inner layers remain framework-agnostic and independently testable. New persistence providers or API transports can be added without modifying business logic. The trade-off is additional project structure and interface indirection for what is currently a single-deployment application.

---

## ADR-2: PostgreSQL

**Status:** Accepted

**Context:** Prior authorization data requires relational integrity—status history, foreign key relationships between patients, providers, payers, and authorization requests—along with strong transactional support for concurrent reviewers.

**Decision:** Use PostgreSQL as the primary data store, accessed through Entity Framework Core with the Npgsql provider. Enable the legacy timestamp behavior switch (`Npgsql.EnableLegacyTimestampBehavior`) to handle .NET 10 DateTime/timestamp mapping correctly.

**Consequences:** Reliable relational storage with full ACID compliance. The legacy timestamp switch is a known workaround for the Npgsql breaking change in timestamp handling and must remain enabled until the codebase migrates to DateTimeOffset throughout.

---

## ADR-3: FHIR R4

**Status:** Accepted

**Context:** The CMS Interoperability and Prior Authorization Final Rule (CMS-0057-F) mandates that payers expose FHIR R4 APIs. The system needs to demonstrate compliance with this healthcare interoperability standard.

**Decision:** Expose FHIR R4 endpoints for Patient, Practitioner, Coverage, and Claim resources using the Firely .NET SDK (`Hl7.Fhir.R4`). A dedicated mapping service converts domain entities to FHIR resource representations.

**Consequences:** The system can interoperate with other FHIR-compliant healthcare systems. Maintaining the mapping layer adds overhead when domain entities change, but isolates FHIR serialization concerns from the core domain.

---

## ADR-4: SignalR for Live Updates

**Status:** Accepted

**Context:** Prior authorization reviewers need real-time visibility into status changes. Polling introduces unnecessary latency and server load, especially when multiple reviewers are monitoring the queue simultaneously.

**Decision:** Use ASP.NET Core SignalR to push status update notifications to connected clients. The hub broadcasts on every status transition, and the React frontend uses these events to invalidate TanStack Query caches and refresh affected views.

**Consequences:** Reviewers see status changes immediately without manual refresh. The WebSocket connection adds server resource overhead per connected client. Graceful fallback to long polling is handled automatically by SignalR when WebSockets are unavailable.

---

## ADR-5: Demo Auth Middleware

**Status:** Accepted (dev/portfolio only)

**Context:** Implementing a full OAuth 2.0 / OIDC authentication flow is outside the scope of this portfolio demonstration. However, the system still needs role-based authorization to show how different user types interact with the prior authorization workflow.

**Decision:** Implement a custom middleware that reads the `X-Demo-Role` HTTP header and constructs a `ClaimsPrincipal` with the specified role. The frontend sets this header via an Axios request interceptor based on the role selected at the login screen. Available roles: Admin, Reviewer, Provider.

**Consequences:** Role-based authorization works throughout the application without external identity provider configuration. This middleware must be replaced with a real OIDC provider (e.g., Azure AD, Auth0) before any production deployment. The `X-Demo-Role` header provides no actual security.

---

## ADR-6: Aggregate Root for Status Transitions

**Status:** Accepted

**Context:** Prior authorization status transitions follow strict business rules—for example, only a Submitted request can move to UnderReview, and a Denied request can only be Appealed, not directly Approved. Scattering these rules across application services risks inconsistency and makes the valid state machine difficult to reason about.

**Decision:** Designate `PriorAuthorizationRequest` as the aggregate root that owns all status transitions. The entity exposes transition methods (e.g., `Submit()`, `Approve()`, `Deny()`) that validate the current state before applying the change and throw a `DomainException` on invalid moves. Application-layer handlers call these methods but do not contain transition logic.

**Consequences:** The complete status state machine is defined in one place, making it easy to audit and test. Domain exceptions provide clear error messages for invalid transitions. The trade-off is that adding new statuses or transitions requires modifying the aggregate root rather than just adding a new handler.
