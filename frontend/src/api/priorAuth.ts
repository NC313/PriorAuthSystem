import client from './client';
import type { StatsDto, PriorAuthSummaryDto, PriorAuthDto, DenialReason, AuditEntry } from '../types';

export interface SubmitPayload {
  patientId: string;
  providerId: string;
  payerId: string;
  icdCode: string;
  icdDescription: string;
  cptCode: string;
  cptDescription: string;
  cptRequiresPriorAuth: boolean;
  clinicalNotes: string;
  clinicalDocumentedBy: string;
  clinicalSupportingDocumentPath: string;
  requiredResponseBy: string;
}

export const getStats = async (): Promise<StatsDto> => {
  const { data } = await client.get('/api/prior-authorizations/stats');
  return data;
};

export const getPendingAuths = async (): Promise<PriorAuthSummaryDto[]> => {
  const { data } = await client.get('/api/prior-authorizations/pending');
  return data;
};

export const getAllAuths = async (): Promise<PriorAuthSummaryDto[]> => {
  const { data } = await client.get('/api/prior-authorizations/all');
  return data;
};

export const getPriorAuthById = async (id: string): Promise<PriorAuthDto> => {
  const { data } = await client.get(`/api/prior-authorizations/${id}`);
  return data;
};

export const getPriorAuthsByPatient = async (patientId: string): Promise<PriorAuthSummaryDto[]> => {
  const { data } = await client.get(`/api/prior-authorizations/patient/${patientId}`);
  return data;
};

export const submitPriorAuth = async (payload: SubmitPayload): Promise<string> => {
  const { data } = await client.post('/api/prior-authorizations', payload);
  return data;
};

export const approvePriorAuth = async (id: string, reviewerId: string, notes: string): Promise<void> => {
  await client.put(`/api/prior-authorizations/${id}/approve`, { reviewerId, notes });
};

export const denyPriorAuth = async (id: string, reviewerId: string, denialReason: DenialReason, notes: string): Promise<void> => {
  await client.put(`/api/prior-authorizations/${id}/deny`, { reviewerId, reason: denialReason, notes });
};

export const requestAdditionalInfo = async (id: string, requestedBy: string, notes: string): Promise<void> => {
  await client.put(`/api/prior-authorizations/${id}/additional-info`, { requestedBy, notes });
};

export const appealPriorAuth = async (id: string, appealedBy: string, clinicalJustification: string): Promise<void> => {
  await client.put(`/api/prior-authorizations/${id}/appeal`, { appealedBy, clinicalJustification });
};

export const getAuditLog = async (): Promise<AuditEntry[]> => {
  const { data } = await client.get('/api/prior-authorizations/audit-log');
  return data;
};

export const getFhirPatient = async (id: string): Promise<unknown> => {
  const { data } = await client.get(`/api/fhir/Patient/${id}`);
  return data;
};

export const getFhirClaim = async (id: string): Promise<unknown> => {
  const { data } = await client.get(`/api/fhir/Claim/${id}`);
  return data;
};
