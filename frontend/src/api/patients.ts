import client from './client';
import type { PatientDto } from '../types';

export const getAllPatients = async (): Promise<PatientDto[]> => {
  const { data } = await client.get('/api/patients');
  return data.map((p: Record<string, unknown>) => ({
    id: p.id,
    firstName: p.firstName,
    lastName: p.lastName,
    fullName: `${p.firstName} ${p.lastName}`,
    dateOfBirth: p.dateOfBirth,
    memberId: p.memberId,
    email: p.email,
    phone: p.phone,
  }));
};

export const getPatientById = async (id: string): Promise<PatientDto> => {
  const { data } = await client.get(`/api/patients/${id}`);
  return {
    ...data,
    fullName: `${data.firstName} ${data.lastName}`,
  };
};

export const getPatientByMemberId = async (memberId: string): Promise<PatientDto> => {
  const { data } = await client.get(`/api/patients/member/${memberId}`);
  return {
    ...data,
    fullName: `${data.firstName} ${data.lastName}`,
  };
};
