const KEY_STORAGE = 'gemini_api_key';

export const getGeminiKey = (): string | null => localStorage.getItem(KEY_STORAGE);
export const setGeminiKey = (key: string) => localStorage.setItem(KEY_STORAGE, key.trim());
export const clearGeminiKey = () => localStorage.removeItem(KEY_STORAGE);

export interface AiAnalysisResult {
  assessment: string;
  likelihood: 'Low' | 'Medium' | 'High';
  concerns: string[];
  recommendation: 'Approve' | 'Deny' | 'RequestAdditionalInfo';
}

export async function analyzeWithGemini(
  patientName: string,
  icdCode: string, icdDescription: string,
  cptCode: string, cptDescription: string,
  payerName: string,
  clinicalJustification: string,
  apiKey: string
): Promise<AiAnalysisResult> {
  const prompt = `You are a clinical reviewer assistant for a prior authorization management system.
Analyze the following prior authorization request and respond ONLY with a valid JSON object — no markdown, no code blocks, just raw JSON.

Prior Authorization Details:
- Patient: ${patientName}
- Diagnosis (ICD-10): ${icdCode} — ${icdDescription}
- Procedure (CPT): ${cptCode} — ${cptDescription}
- Payer: ${payerName}
- Clinical Justification: ${clinicalJustification}

Respond with this exact JSON structure:
{
  "assessment": "2-3 sentence clinical assessment of whether the justification supports medical necessity",
  "likelihood": "Low or Medium or High",
  "concerns": ["concern 1", "concern 2"],
  "recommendation": "Approve or Deny or RequestAdditionalInfo"
}`;

  const res = await fetch(
    `https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=${apiKey}`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ contents: [{ parts: [{ text: prompt }] }] }),
    }
  );

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    const msg = err?.error?.message ?? res.statusText;
    throw new Error(msg);
  }

  const data = await res.json();
  const text: string = data.candidates?.[0]?.content?.parts?.[0]?.text ?? '';

  // Extract JSON from anywhere in the response (handles extra prose + code fences)
  const match = text.match(/\{[\s\S]*\}/);
  if (!match) throw new Error('AI returned an unreadable response. Please try again.');
  const result = JSON.parse(match[0]) as AiAnalysisResult;

  // Normalise casing so TypeScript union types are satisfied
  result.likelihood = (result.likelihood as string).charAt(0).toUpperCase() +
    (result.likelihood as string).slice(1).toLowerCase() as AiAnalysisResult['likelihood'];
  result.recommendation = (result.recommendation as string).charAt(0).toUpperCase() +
    (result.recommendation as string).slice(1) as AiAnalysisResult['recommendation'];

  return result;
}
