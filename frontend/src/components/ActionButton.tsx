import type { ButtonHTMLAttributes } from 'react';

type Variant = 'primary' | 'danger' | 'warning' | 'secondary' | 'ghost';

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  loading?: boolean;
  fullWidth?: boolean;
}

const variantStyles: Record<Variant, { bg: string; color: string; border: string }> = {
  primary: { bg: 'var(--green)', color: 'var(--white)', border: 'var(--green)' },
  danger: { bg: 'var(--red)', color: 'var(--white)', border: 'var(--red)' },
  warning: { bg: 'var(--amber)', color: 'var(--white)', border: 'var(--amber)' },
  secondary: { bg: 'var(--navy)', color: 'var(--white)', border: 'var(--navy)' },
  ghost: { bg: 'transparent', color: 'var(--gray-600)', border: 'var(--gray-300)' },
};

export default function ActionButton({ variant = 'primary', loading, fullWidth, disabled, children, style, ...props }: Props) {
  const v = variantStyles[variant];
  return (
    <button
      disabled={disabled || loading}
      style={{
        display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: 8,
        padding: '8px 20px', borderRadius: 'var(--radius)', fontSize: 13, fontWeight: 600,
        border: `1px solid ${v.border}`, background: v.bg, color: v.color,
        opacity: disabled || loading ? 0.6 : 1,
        cursor: disabled || loading ? 'not-allowed' : 'pointer',
        transition: 'var(--transition)',
        width: fullWidth ? '100%' : 'auto',
        ...style,
      }}
      {...props}
    >
      {loading && (
        <span style={{
          width: 14, height: 14, border: '2px solid transparent',
          borderTopColor: v.color, borderRadius: '50%',
          animation: 'spin 0.6s linear infinite', display: 'inline-block',
        }} />
      )}
      {children}
      <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
    </button>
  );
}
