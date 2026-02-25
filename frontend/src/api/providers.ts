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
