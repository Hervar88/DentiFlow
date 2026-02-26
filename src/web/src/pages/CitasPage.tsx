import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getClinicaProfile, getAppointments, bookAppointment, updateAppointmentStatus, cancelAppointment, getDentistas, type Cita } from '../api';
import { CLINICA_SLUG } from '../App';
import { Plus, X } from 'lucide-react';

const estadoColor: Record<string, string> = {
  Pendiente: 'bg-yellow-100 text-yellow-800',
  Confirmada: 'bg-blue-100 text-blue-800',
  Pagada: 'bg-green-100 text-green-800',
  EnProgreso: 'bg-purple-100 text-purple-800',
  Completada: 'bg-gray-100 text-gray-800',
  Cancelada: 'bg-red-100 text-red-800',
  NoAsistio: 'bg-orange-100 text-orange-800',
};

const ESTADOS = ['Pendiente', 'Confirmada', 'Pagada', 'EnProgreso', 'Completada', 'Cancelada', 'NoAsistio'];

export default function CitasPage() {
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);

  const { data: clinica } = useQuery({
    queryKey: ['clinica', CLINICA_SLUG],
    queryFn: () => getClinicaProfile(CLINICA_SLUG),
  });

  const hoy = new Date();
  const desde = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate() - 7).toISOString();
  const hasta = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate() + 30).toISOString();

  const { data: citas = [] } = useQuery({
    queryKey: ['appointments', clinica?.id, desde, hasta],
    queryFn: () => getAppointments(clinica!.id, desde, hasta),
    enabled: !!clinica?.id,
  });

  const { data: dentistas = [] } = useQuery({
    queryKey: ['dentistas', clinica?.id],
    queryFn: () => getDentistas(clinica!.id),
    enabled: !!clinica?.id,
  });

  const statusMutation = useMutation({
    mutationFn: ({ id, estado }: { id: string; estado: string }) => updateAppointmentStatus(id, estado),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['appointments'] }),
  });

  const cancelMutation = useMutation({
    mutationFn: cancelAppointment,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['appointments'] }),
  });

  const bookMutation = useMutation({
    mutationFn: bookAppointment,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      setShowForm(false);
    },
  });

  const handleBook = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    bookMutation.mutate({
      clinicaId: clinica!.id,
      dentistaId: fd.get('dentistaId') as string,
      nombrePaciente: fd.get('nombrePaciente') as string,
      apellidoPaciente: fd.get('apellidoPaciente') as string,
      emailPaciente: (fd.get('emailPaciente') as string) || undefined,
      telefonoPaciente: (fd.get('telefonoPaciente') as string) || undefined,
      fechaHora: new Date(fd.get('fechaHora') as string).toISOString(),
      duracionMinutos: Number(fd.get('duracionMinutos')) || 30,
      motivo: (fd.get('motivo') as string) || undefined,
    });
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Gestión de Citas</h1>
        <button
          onClick={() => setShowForm(!showForm)}
          className="flex items-center gap-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition text-sm font-medium"
        >
          {showForm ? <X size={16} /> : <Plus size={16} />}
          {showForm ? 'Cancelar' : 'Nueva Cita'}
        </button>
      </div>

      {/* New appointment form */}
      {showForm && (
        <form onSubmit={handleBook} className="bg-white rounded-xl border shadow-sm p-6 mb-6 grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Dentista *</label>
            <select name="dentistaId" required className="w-full border rounded-lg px-3 py-2 text-sm">
              <option value="">Seleccionar...</option>
              {dentistas.map(d => (
                <option key={d.id} value={d.id}>{d.nombre} {d.apellido} — {d.especialidad}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Fecha y Hora *</label>
            <input type="datetime-local" name="fechaHora" required className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Nombre del Paciente *</label>
            <input type="text" name="nombrePaciente" required className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Apellido del Paciente *</label>
            <input type="text" name="apellidoPaciente" required className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input type="email" name="emailPaciente" className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Teléfono</label>
            <input type="tel" name="telefonoPaciente" className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Duración (min)</label>
            <input type="number" name="duracionMinutos" defaultValue={30} min={15} step={15} className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Motivo</label>
            <input type="text" name="motivo" className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div className="md:col-span-2">
            <button type="submit" disabled={bookMutation.isPending} className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition text-sm font-medium disabled:opacity-50">
              {bookMutation.isPending ? 'Agendando...' : 'Agendar Cita'}
            </button>
            {bookMutation.isError && (
              <p className="mt-2 text-sm text-red-600">{(bookMutation.error as Error).message}</p>
            )}
          </div>
        </form>
      )}

      {/* Appointments Table */}
      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Fecha</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Hora</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Paciente</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Dentista</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Motivo</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Estado</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {citas.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-4 py-12 text-center text-gray-400">No hay citas en este rango.</td>
              </tr>
            ) : (
              citas.map((cita) => <CitaTableRow key={cita.id} cita={cita} onStatusChange={(estado) => statusMutation.mutate({ id: cita.id, estado })} onCancel={() => cancelMutation.mutate(cita.id)} />)
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function CitaTableRow({ cita, onStatusChange, onCancel }: { cita: Cita; onStatusChange: (estado: string) => void; onCancel: () => void }) {
  const fecha = new Date(cita.fechaHora);
  const isCancelled = cita.estado === 'Cancelada';

  return (
    <tr className={`hover:bg-gray-50 ${isCancelled ? 'opacity-50' : ''}`}>
      <td className="px-4 py-3">{fecha.toLocaleDateString('es-MX', { day: '2-digit', month: 'short' })}</td>
      <td className="px-4 py-3">{fecha.toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' })}</td>
      <td className="px-4 py-3 font-medium">{cita.nombrePaciente}</td>
      <td className="px-4 py-3">{cita.nombreDentista}</td>
      <td className="px-4 py-3 text-gray-500">{cita.motivo || '—'}</td>
      <td className="px-4 py-3">
        <select
          value={cita.estado}
          onChange={(e) => onStatusChange(e.target.value)}
          disabled={isCancelled}
          className={`text-xs rounded-full px-2 py-1 font-medium border-0 ${estadoColor[cita.estado] ?? 'bg-gray-100'}`}
        >
          {ESTADOS.map(s => <option key={s} value={s}>{s}</option>)}
        </select>
      </td>
      <td className="px-4 py-3">
        {!isCancelled && (
          <button onClick={onCancel} className="text-xs text-red-600 hover:text-red-800 font-medium">
            Cancelar
          </button>
        )}
      </td>
    </tr>
  );
}
