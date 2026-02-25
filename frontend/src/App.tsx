import { Routes, Route, Navigate } from 'react-router-dom';
import AppShell from './layout/AppShell';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import ReviewQueue from './pages/ReviewQueue';
import Submit from './pages/Submit';
import Patients from './pages/Patients';
import Providers from './pages/Providers';
import Payers from './pages/Payers';
import Reports from './pages/Reports';
import AuditLog from './pages/AuditLog';
import FhirExplorer from './pages/FhirExplorer';
import Detail from './pages/Detail';
import NotFound from './pages/NotFound';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const role = localStorage.getItem('demo_role');
  if (!role) {
    return <Navigate to="/" replace />;
  }
  return <>{children}</>;
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Login />} />
      <Route
        path="/app"
        element={
          <ProtectedRoute>
            <AppShell />
          </ProtectedRoute>
        }
      >
        <Route index element={<Dashboard />} />
        <Route path="review-queue" element={<ReviewQueue />} />
        <Route path="submit" element={<Submit />} />
        <Route path="patients" element={<Patients />} />
        <Route path="providers" element={<Providers />} />
        <Route path="payers" element={<Payers />} />
        <Route path="reports" element={<Reports />} />
        <Route path="audit" element={<AuditLog />} />
        <Route path="fhir" element={<FhirExplorer />} />
        <Route path="auth/:id" element={<Detail />} />
      </Route>
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}
