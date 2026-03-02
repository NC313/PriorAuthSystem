import client from './client';
import type { ProviderDto } from '../types';

export const getAllProviders = async (): Promise<ProviderDto[]> => {
  const { data } = await client.get('/api/providers');
  return data.map((p: Record<string, unknown>) => ({
    id: p.id,
    firstName: p.firstName,
    lastName: p.lastName,
    fullName: `Dr. ${p.firstName} ${p.lastName}`,
    npi: p.npi ?? p.nPI,
    specialty: p.specialty,
    email: p.email,
    phone: p.phone,
  }));
};

export const createProvider = async (payload: {
  firstName: string; lastName: string; npi: string;
  specialty: string; organizationName?: string;
  phone: string; email: string; faxNumber?: string;
}): Promise<ProviderDto> => {
  const { data } = await client.post('/api/providers', payload);
  return { ...data, fullName: `Dr. ${data.firstName} ${data.lastName}`, npi: data.npi ?? data.nPI };
};
