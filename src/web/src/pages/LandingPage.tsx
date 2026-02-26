import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { getClinicaProfile, bookAppointment } from '../api';
import { MapPin, Phone, CheckCircle } from 'lucide-react';

export default function LandingPage() {
  const { slug } = useParams<{ slug: string }>();
  const [showBooking, setShowBooking] = useState(false);
  const [booked, setBooked] = useState(false);

  const { data: clinica, isLoading, isError } = useQuery({
    queryKey: ['clinica', slug],
    queryFn: () => getClinicaProfile(slug!),
    enabled: !!slug,
  });

  const bookMutation = useMutation({
    mutationFn: bookAppointment,
    onSuccess: () => setBooked(true),
  });

  const handleBook = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!clinica) return;
    const fd = new FormData(e.currentTarget);
    bookMutation.mutate({
      clinicaId: clinica.id,
      dentistaId: fd.get('dentistaId') as string,
      nombrePaciente: fd.get('nombre') as string,
      apellidoPaciente: fd.get('apellido') as string,
      emailPaciente: (fd.get('email') as string) || undefined,
      telefonoPaciente: (fd.get('telefono') as string) || undefined,
      fechaHora: new Date(fd.get('fechaHora') as string).toISOString(),
      duracionMinutos: 30,
      motivo: (fd.get('motivo') as string) || undefined,
    });
  };

  if (isLoading) return <LoadingScreen />;
  if (isError || !clinica) return <NotFoundScreen />;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm">
        <div className="max-w-4xl mx-auto px-4 py-6 flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">{clinica.nombre}</h1>
            <div className="flex items-center gap-4 mt-1 text-sm text-gray-500">
              {clinica.direccion && (
                <span className="flex items-center gap-1"><MapPin size={14} /> {clinica.direccion}</span>
              )}
              {clinica.telefono && (
                <span className="flex items-center gap-1"><Phone size={14} /> {clinica.telefono}</span>
              )}
            </div>
          </div>
          <button
            onClick={() => { setShowBooking(true); setBooked(false); }}
            className="px-5 py-2.5 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition"
          >
            Agendar Cita
          </button>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-10">
        {/* Descripción */}
        {clinica.descripcion && (
          <section className="mb-10">
            <p className="text-gray-600 text-lg leading-relaxed">{clinica.descripcion}</p>
          </section>
        )}

        {/* Especialidades */}
        <section className="mb-10">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Nuestros Servicios</h2>
          <div className="flex flex-wrap gap-2">
            {clinica.especialidades.map((esp) => (
              <span key={esp} className="px-4 py-2 bg-blue-50 text-blue-700 rounded-full text-sm font-medium">
                {esp}
              </span>
            ))}
          </div>
        </section>

        {/* Dentistas */}
        <section className="mb-10">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Nuestros Especialistas</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {clinica.dentistas.map((d) => (
              <div key={d.id} className="bg-white rounded-xl border p-5 flex items-center gap-4">
                <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center text-blue-600 font-bold text-lg">
                  {d.nombre[0]}{d.apellido[0]}
                </div>
                <div>
                  <p className="font-medium text-gray-900">Dr. {d.nombre} {d.apellido}</p>
                  <p className="text-sm text-gray-500">{d.especialidad || 'Odontología General'}</p>
                </div>
              </div>
            ))}
          </div>
        </section>

        {/* Booking Form Modal */}
        {showBooking && (
          <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-2xl shadow-xl max-w-lg w-full p-6 relative">
              <button onClick={() => setShowBooking(false)} className="absolute top-4 right-4 text-gray-400 hover:text-gray-600 text-xl">&times;</button>

              {booked ? (
                <div className="text-center py-8">
                  <CheckCircle size={48} className="text-green-500 mx-auto mb-4" />
                  <h3 className="text-xl font-semibold text-gray-900">¡Cita agendada!</h3>
                  <p className="text-gray-500 mt-2">Te confirmaremos por correo o WhatsApp.</p>
                  <button onClick={() => setShowBooking(false)} className="mt-6 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition">
                    Cerrar
                  </button>
                </div>
              ) : (
                <>
                  <h3 className="text-xl font-semibold text-gray-900 mb-4">Agendar Cita</h3>
                  <form onSubmit={handleBook} className="space-y-3">
                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Nombre *</label>
                        <input type="text" name="nombre" required className="w-full border rounded-lg px-3 py-2 text-sm" />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Apellido *</label>
                        <input type="text" name="apellido" required className="w-full border rounded-lg px-3 py-2 text-sm" />
                      </div>
                    </div>
                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                        <input type="email" name="email" className="w-full border rounded-lg px-3 py-2 text-sm" />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Teléfono</label>
                        <input type="tel" name="telefono" className="w-full border rounded-lg px-3 py-2 text-sm" />
                      </div>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Especialista *</label>
                      <select name="dentistaId" required className="w-full border rounded-lg px-3 py-2 text-sm">
                        <option value="">Seleccionar...</option>
                        {clinica.dentistas.map(d => (
                          <option key={d.id} value={d.id}>Dr. {d.nombre} {d.apellido} — {d.especialidad || 'General'}</option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Fecha y Hora *</label>
                      <input type="datetime-local" name="fechaHora" required className="w-full border rounded-lg px-3 py-2 text-sm" />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Motivo</label>
                      <input type="text" name="motivo" placeholder="Ej. Limpieza dental" className="w-full border rounded-lg px-3 py-2 text-sm" />
                    </div>
                    <button type="submit" disabled={bookMutation.isPending} className="w-full py-2.5 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition disabled:opacity-50">
                      {bookMutation.isPending ? 'Agendando...' : 'Confirmar Cita'}
                    </button>
                    {bookMutation.isError && (
                      <p className="text-sm text-red-600">{(bookMutation.error as Error).message}</p>
                    )}
                  </form>
                </>
              )}
            </div>
          </div>
        )}
      </main>

      <footer className="border-t bg-white mt-16">
        <div className="max-w-4xl mx-auto px-4 py-6 text-center text-gray-400 text-sm">
          Powered by DentiFlow
        </div>
      </footer>
    </div>
  );
}

function LoadingScreen() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600" />
    </div>
  );
}

function NotFoundScreen() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center text-gray-500">
      <p className="text-6xl mb-4">🦷</p>
      <h2 className="text-2xl font-bold text-gray-900 mb-2">Clínica no encontrada</h2>
      <p>Verifica la URL e intenta de nuevo.</p>
    </div>
  );
}
