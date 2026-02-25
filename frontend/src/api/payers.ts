import client from './client';
import type { PayerDto } from '../types';

export const getAllPayers = async (): Promise<PayerDto[]> => {
  const { data } = await client.get('/api/payers');
  return data.map((p: Record<string, unknown>) => ({
    id: p.id,
    name: p.payerName,
    payerId: p.payerId,
    standardResponseDays: p.standardResponseDays,
    phone: p.phone,
    email: p.email,
  }));
};
