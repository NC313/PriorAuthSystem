import { Component, type ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  message: string;
}

export default class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, message: '' };
  }

  static getDerivedStateFromError(error: unknown): State {
    const message = error instanceof Error ? error.message : 'An unexpected error occurred.';
    return { hasError: true, message };
  }

  override render() {
    if (this.state.hasError) {
      return (
        <div style={{
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          minHeight: '100vh', background: 'var(--gray-50)',
        }}>
          <div style={{
            background: 'var(--white)', borderRadius: 'var(--radius-lg)',
            boxShadow: 'var(--shadow)', padding: 40, maxWidth: 480,
            textAlign: 'center',
          }}>
            <div style={{ fontSize: 48, marginBottom: 16 }}>⚠️</div>
            <h2 style={{ fontSize: 18, fontWeight: 700, marginBottom: 8, color: 'var(--gray-900)' }}>
              Something went wrong
            </h2>
            <p style={{ fontSize: 13, color: 'var(--gray-500)', marginBottom: 24 }}>
              {this.state.message}
            </p>
            <button
              onClick={() => window.location.reload()}
              style={{
                padding: '10px 24px', borderRadius: 'var(--radius)', border: 'none',
                background: 'var(--navy)', color: 'var(--white)', fontSize: 13,
                fontWeight: 600, cursor: 'pointer',
              }}
            >
              Reload page
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
