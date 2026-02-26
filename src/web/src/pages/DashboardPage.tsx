import { useQuery } from '@tanstack/react-query';
import { getClinicaProfile, getAppointments, type Cita } from '../api';
import { CLINICA_SLUG } from '../App';
import { CalendarDays, Users, Clock, CheckCircle } from 'lucide-react';

const estadoColor: Record<string, string> = {
  Pendiente: 'bg-yellow-100 text-yellow-800',
  Confirmada: 'bg-blue-100 text-blue-800',
  Pagada: 'bg-green-100 text-green-800',
  EnProgreso: 'bg-purple-100 text-purple-800',
  Completada: 'bg-gray-100 text-gray-800',
  Cancelada: 'bg-red-100 text-red-800',
  NoAsistio: 'bg-orange-100 text-orange-800',
};

export default function DashboardPage() {
  const { data: clinica } = useQuery({
    queryKey: ['clinica', CLINICA_SLUG],
    queryFn: () => getClinicaProfile(CLINICA_SLUG),
  });

  const hoy = new Date();
  const desde = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate()).toISOString();
  const hasta = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate() + 7).toISOString();

  const { data: citas = [] } = useQuery({
    queryKey: ['appointments', clinica?.id, desde, hasta],
    queryFn: () => getAppointments(clinica!.id, desde, hasta),
    enabled: !!clinica?.id,
  });

  const citasHoy = citas.filter(c => {
    const fecha = new Date(c.fechaHora);
    return fecha.toDateString() === hoy.toDateString();
  });

  const citasPendientes = citas.filter(c => c.estado === 'Pendiente').length;
  const citasConfirmadas = citas.filter(c => c.estado === 'Confirmada' || c.estado === 'Pagada').length;

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">
        Dashboard — {clinica?.nombre || 'Cargando...'}
      </h1>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        <StatCard icon={<CalendarDays className="text-blue-500" />} label="Citas hoy" value={citasHoy.length} />
        <StatCard icon={<Clock className="text-yellow-500" />} label="Pendientes (7d)" value={citasPendientes} />
        <StatCard icon={<CheckCircle className="text-green-500" />} label="Confirmadas (7d)" value={citasConfirmadas} />
        <StatCard icon={<Users className="text-purple-500" />} label="Dentistas" value={clinica?.dentistas.length ?? 0} />
      </div>

      {/* Upcoming appointments */}
      <div className="bg-white rounded-xl border shadow-sm">
        <div className="px-6 py-4 border-b">
          <h2 className="text-lg font-semibold text-gray-900">Próximas citas (7 días)</h2>
        </div>
        {citas.length === 0 ? (
          <div className="px-6 py-12 text-center text-gray-400">
            No hay citas programadas para los próximos 7 días.
          </div>
        ) : (
          <div className="divide-y">
            {citas.map((cita) => (
              <CitaRow key={cita.id} cita={cita} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function StatCard({ icon, label, value }: { icon: React.ReactNode; label: string; value: number }) {
  return (
    <div className="bg-white rounded-xl border shadow-sm p-4 flex items-center gap-4">
      <div className="p-2 bg-gray-50 rounded-lg">{icon}</div>
      <div>
        <p className="text-2xl font-bold text-gray-900">{value}</p>
        <p className="text-sm text-gray-500">{label}</p>
      </div>
    </div>
  );
}

function CitaRow({ cita }: { cita: Cita }) {
  const fecha = new Date(cita.fechaHora);
  return (
    <div className="flex items-center justify-between px-6 py-3 hover:bg-gray-50">
      <div className="flex items-center gap-4">
        <div className="text-center min-w-[50px]">
          <p className="text-xs text-gray-400">
            {fecha.toLocaleDateString('es-MX', { weekday: 'short', day: 'numeric' })}
          </p>
          <p className="text-sm font-semibold text-gray-700">
            {fecha.toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' })}
          </p>
        </div>
        <div>
          <p className="text-sm font-medium text-gray-900">{cita.nombrePaciente}</p>
          <p className="text-xs text-gray-500">Dr. {cita.nombreDentista} — {cita.motivo || 'Sin motivo'}</p>
        </div>
      </div>
      <div className="flex items-center gap-3">
        <span className="text-xs text-gray-400">{cita.duracionMinutos} min</span>
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${estadoColor[cita.estado] ?? 'bg-gray-100'}`}>
          {cita.estado}
        </span>
      </div>
    </div>
  );
}
