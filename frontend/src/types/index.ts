export type PriorAuthStatus =
  'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Denied' |
  'AdditionalInfoRequested' | 'Appealed' | 'AppealApproved' | 'AppealDenied' | 'Expired';

export type DenialReason =
  'NotMedicallyNecessary' | 'ServiceNotCovered' | 'RequiresAlternativeTreatment' |
  'InsufficientDocumentation' | 'OutOfNetwork' | 'DuplicateRequest' |
  'EligibilityIssue' | 'PriorAuthNotRequired' | 'Other';

export interface PatientDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  memberId: string;
  email: string;
  phone: string;
}

export interface ProviderDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  npi: string;
  specialty: string;
  email: string;
}

export interface PayerDto {
  id: string;
  name: string;
  payerId: string;
  standardResponseDays: number;
  phone: string;
  email: string;
}

export interface StatusTransitionDto {
  fromStatus: PriorAuthStatus;
  toStatus: PriorAuthStatus;
  transitionedBy: string;
  notes: string;
  transitionedAt: string;
}

export interface PriorAuthPatientDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  memberId: string;
  email: string;
  phone: string;
}

export interface PriorAuthProviderDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  npi: string;
  specialty: string;
  email: string;
}

export interface PriorAuthPayerDto {
  id: string;
  name: string;
  payerId: string;
  standardResponseDays: number;
  phone: string;
  email: string;
}

export interface PriorAuthDto {
  id: string;
  patient: PriorAuthPatientDto;
  provider: PriorAuthProviderDto;
  payer: PriorAuthPayerDto;
  icdCode: string;
  icdDescription: string;
  cptCode: string;
  cptDescription: string;
  clinicalJustification: string;
  status: PriorAuthStatus;
  denialReason?: DenialReason;
  submittedAt: string;
  requiredResponseBy: string;
  statusTransitions: StatusTransitionDto[];
}

export interface PriorAuthSummaryDto {
  id: string;
  patientName: string;
  providerName: string;
  payerName: string;
  cptCode: string;
  icdCode: string;
  status: PriorAuthStatus;
  submittedAt: string;
  requiredResponseBy: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface StatsDto {
  pending: number;
  approved: number;
  denied: number;
  underReview: number;
  avgResponseDays: number;
  denialReasonBreakdown: Record<string, number>;
}

export interface AuditEntry {
  timestamp: string;
  action: string;
  performedBy: string;
  requestId: string;
  details: string;
}

export type DemoRole = 'Admin' | 'Reviewer' | 'Provider';

export interface DemoUser {
  role: DemoRole;
  name: string;
  userId: string;
}
