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

export const createPayer = async (payload: {
  payerName: string; payerId: string; standardResponseDays: number;
  phone: string; email: string; faxNumber?: string;
}): Promise<PayerDto> => {
  const { data } = await client.post('/api/payers', payload);
  return {
    id: data.id, name: data.payerName, payerId: data.payerId,
    standardResponseDays: data.standardResponseDays,
    phone: data.phone, email: data.email,
  };
};
