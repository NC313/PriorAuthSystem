import { useState, useCallback, useEffect, createContext, useContext, type ReactNode } from 'react';

interface ToastMessage {
  id: number;
  text: string;
  type: 'success' | 'error';
}

interface ToastContextType {
  showToast: (text: string, type: 'success' | 'error') => void;
}

const ToastContext = createContext<ToastContextType>({ showToast: () => {} });

export const useToast = () => useContext(ToastContext);

let nextId = 0;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const showToast = useCallback((text: string, type: 'success' | 'error') => {
    const id = ++nextId;
    setToasts(prev => [...prev, { id, text, type }]);
  }, []);

  const removeToast = useCallback((id: number) => {
    setToasts(prev => prev.filter(t => t.id !== id));
  }, []);

  return (
    <ToastContext.Provider value={{ showToast }}>
      {children}
      <div style={{ position: 'fixed', bottom: 20, right: 20, zIndex: 2000, display: 'flex', flexDirection: 'column', gap: 8 }}>
        {toasts.map(t => (
          <ToastItem key={t.id} toast={t} onRemove={removeToast} />
        ))}
      </div>
    </ToastContext.Provider>
  );
}

function ToastItem({ toast, onRemove }: { toast: ToastMessage; onRemove: (id: number) => void }) {
  useEffect(() => {
    const timer = setTimeout(() => onRemove(toast.id), 4000);
    return () => clearTimeout(timer);
  }, [toast.id, onRemove]);

  return (
    <div style={{
      padding: '12px 20px', borderRadius: 'var(--radius)',
      background: toast.type === 'success' ? 'var(--green)' : 'var(--red)',
      color: 'var(--white)', fontSize: 13, fontWeight: 500,
      boxShadow: 'var(--shadow-md)', minWidth: 280, maxWidth: 400,
      display: 'flex', justifyContent: 'space-between', alignItems: 'center',
    }}>
      <span>{toast.text}</span>
      <button onClick={() => onRemove(toast.id)} style={{
        background: 'none', border: 'none', color: 'var(--white)',
        fontSize: 16, marginLeft: 12, cursor: 'pointer',
      }}>x</button>
    </div>
  );
}
