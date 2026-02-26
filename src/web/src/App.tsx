import { BrowserRouter, Routes, Route, Link, useLocation } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Calendar, Users, LayoutDashboard, Globe } from 'lucide-react';
import DashboardPage from './pages/DashboardPage';
import PacientesPage from './pages/PacientesPage';
import CitasPage from './pages/CitasPage';
import LandingPage from './pages/LandingPage';

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000 } },
});

// Hardcoded for MVP — later this comes from auth
export const CLINICA_SLUG = 'dental-sonrisa-mx';

function NavLink({ to, children }: { to: string; children: React.ReactNode }) {
  const { pathname } = useLocation();
  const active = pathname === to;
  return (
    <Link
      to={to}
      className={`flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition ${
        active ? 'bg-blue-100 text-blue-700' : 'text-gray-600 hover:bg-gray-100'
      }`}
    >
      {children}
    </Link>
  );
}

function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-gray-50 flex">
      {/* Sidebar */}
      <aside className="w-64 bg-white border-r p-4 flex flex-col gap-1">
        <div className="flex items-center gap-2 mb-6 px-3">
          <span className="text-2xl">🦷</span>
          <span className="text-xl font-bold text-blue-600">DentiFlow</span>
        </div>
        <NavLink to="/dashboard">
          <LayoutDashboard size={18} /> Dashboard
        </NavLink>
        <NavLink to="/dashboard/citas">
          <Calendar size={18} /> Citas
        </NavLink>
        <NavLink to="/dashboard/pacientes">
          <Users size={18} /> Pacientes
        </NavLink>
        <div className="mt-auto pt-4 border-t">
          <NavLink to={`/clinica/${CLINICA_SLUG}`}>
            <Globe size={18} /> Ver Landing
          </NavLink>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 p-6 overflow-auto">
        {children}
      </main>
    </div>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Public Landing */}
          <Route path="/clinica/:slug" element={<LandingPage />} />

          {/* Dashboard */}
          <Route path="/dashboard" element={<DashboardLayout><DashboardPage /></DashboardLayout>} />
          <Route path="/dashboard/citas" element={<DashboardLayout><CitasPage /></DashboardLayout>} />
          <Route path="/dashboard/pacientes" element={<DashboardLayout><PacientesPage /></DashboardLayout>} />

          {/* Redirect home to dashboard */}
          <Route path="*" element={<DashboardLayout><DashboardPage /></DashboardLayout>} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
