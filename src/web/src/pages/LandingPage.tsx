import { useState, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { getClinicaProfile, bookAppointment } from '../api';
import {
  MapPin, Phone, CheckCircle, Star, Shield, Clock, Heart,
  Sparkles, Smile, Stethoscope, CalendarCheck, ArrowRight, X,
} from 'lucide-react';

/* ── Service icon mapping ── */
const SERVICE_META: Record<string, { icon: React.ReactNode; desc: string }> = {
  Ortodoncia:       { icon: <Smile className="w-7 h-7" />,        desc: 'Brackets, alineadores invisibles y corrección de la mordida para una sonrisa perfecta.' },
  Implantes:        { icon: <Shield className="w-7 h-7" />,       desc: 'Implantes de titanio de última generación con integración ósea garantizada.' },
  Endodoncia:       { icon: <Stethoscope className="w-7 h-7" />,  desc: 'Tratamientos de conducto sin dolor con tecnología avanzada y anestesia localizada.' },
  'Estética Dental': { icon: <Sparkles className="w-7 h-7" />,    desc: 'Carillas, blanqueamiento láser y diseño digital de sonrisa personalizado.' },
  Limpieza:         { icon: <Heart className="w-7 h-7" />,        desc: 'Profilaxis profesional, eliminación de sarro y pulido para una higiene impecable.' },
};
const DEFAULT_SERVICE = { icon: <Star className="w-7 h-7" />, desc: 'Servicio odontológico profesional con los más altos estándares de calidad.' };

export default function LandingPage() {
  const { slug } = useParams<{ slug: string }>();
  const [showBooking, setShowBooking] = useState(false);
  const [booked, setBooked] = useState(false);
  const servicesRef = useRef<HTMLElement>(null);
  const teamRef = useRef<HTMLElement>(null);

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

  const scrollTo = (ref: React.RefObject<HTMLElement | null>) =>
    ref.current?.scrollIntoView({ behavior: 'smooth' });

  if (isLoading) return <LoadingScreen />;
  if (isError || !clinica) return <NotFoundScreen />;

  return (
    <div className="min-h-screen bg-white text-gray-900 overflow-x-hidden">

      {/* ═══════════════════════════ NAV ═══════════════════════════ */}
      <nav className="fixed top-0 inset-x-0 z-40 bg-white/80 backdrop-blur-lg border-b border-gray-100">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 h-16 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="text-2xl">🦷</span>
            <span className="text-lg font-bold tracking-tight bg-gradient-to-r from-sky-600 to-cyan-500 bg-clip-text text-transparent">
              {clinica.nombre}
            </span>
          </div>
          <div className="hidden sm:flex items-center gap-6 text-sm font-medium text-gray-500">
            <button onClick={() => scrollTo(servicesRef)} className="hover:text-gray-900 transition">Servicios</button>
            <button onClick={() => scrollTo(teamRef)} className="hover:text-gray-900 transition">Equipo</button>
            {clinica.telefono && (
              <a href={`tel:${clinica.telefono}`} className="flex items-center gap-1 hover:text-gray-900 transition">
                <Phone size={14} /> {clinica.telefono}
              </a>
            )}
          </div>
          <button
            onClick={() => { setShowBooking(true); setBooked(false); }}
            className="px-4 py-2 text-sm font-semibold rounded-full bg-gradient-to-r from-sky-600 to-cyan-500 text-white shadow-lg shadow-sky-500/25 hover:shadow-sky-500/40 transition-all hover:scale-105"
          >
            Agendar Cita
          </button>
        </div>
      </nav>

      {/* ═══════════════════════════ HERO ═══════════════════════════ */}
      <section className="relative pt-32 pb-20 sm:pt-40 sm:pb-28 overflow-hidden">
        {/* Background decoration */}
        <div className="absolute inset-0 -z-10">
          <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-sky-100 rounded-full -translate-y-1/2 translate-x-1/3 blur-3xl opacity-60" />
          <div className="absolute bottom-0 left-0 w-[400px] h-[400px] bg-cyan-100 rounded-full translate-y-1/2 -translate-x-1/3 blur-3xl opacity-50" />
        </div>

        <div className="max-w-6xl mx-auto px-4 sm:px-6 text-center">
          <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-sky-50 border border-sky-200 text-sky-700 text-sm font-medium mb-6">
            <Sparkles size={14} /> Más de 10 años cuidando sonrisas
          </div>

          <h1 className="text-4xl sm:text-5xl lg:text-6xl font-extrabold leading-tight tracking-tight">
            Tu sonrisa merece{' '}
            <span className="bg-gradient-to-r from-sky-600 to-cyan-500 bg-clip-text text-transparent">
              lo mejor
            </span>
          </h1>

          <p className="mt-6 text-lg sm:text-xl text-gray-500 max-w-2xl mx-auto leading-relaxed">
            {clinica.descripcion ||
              `En ${clinica.nombre} combinamos tecnología de última generación con un trato humano y cercano para que cada visita sea una experiencia confortable.`}
          </p>

          <div className="mt-10 flex flex-col sm:flex-row items-center justify-center gap-4">
            <button
              onClick={() => { setShowBooking(true); setBooked(false); }}
              className="group px-7 py-3.5 text-base font-semibold rounded-full bg-gradient-to-r from-sky-600 to-cyan-500 text-white shadow-xl shadow-sky-500/25 hover:shadow-sky-500/40 transition-all hover:scale-105 flex items-center gap-2"
            >
              <CalendarCheck size={18} />
              Agendar Cita Ahora
              <ArrowRight size={16} className="transition-transform group-hover:translate-x-1" />
            </button>
            <button
              onClick={() => scrollTo(servicesRef)}
              className="px-7 py-3.5 text-base font-semibold rounded-full border-2 border-gray-200 text-gray-700 hover:border-sky-300 hover:text-sky-600 transition-all"
            >
              Ver Servicios
            </button>
          </div>

          {/* Trust badges */}
          <div className="mt-14 flex flex-wrap items-center justify-center gap-x-8 gap-y-3 text-sm text-gray-400">
            <span className="flex items-center gap-1.5"><Shield size={16} className="text-sky-500" /> Certificaciones vigentes</span>
            <span className="flex items-center gap-1.5"><Star size={16} className="text-amber-400" /> 4.9 ★ en Google</span>
            <span className="flex items-center gap-1.5"><Clock size={16} className="text-sky-500" /> Citas en 24h</span>
          </div>
        </div>
      </section>

      {/* ═══════════════════════════ STATS ═══════════════════════════ */}
      <section className="border-y border-gray-100 bg-gray-50/50">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 py-12 grid grid-cols-2 sm:grid-cols-4 gap-8 text-center">
          {[
            { value: '10+', label: 'Años de experiencia' },
            { value: `${clinica.dentistas.length}`, label: 'Especialistas' },
            { value: `${clinica.especialidades.length}`, label: 'Servicios' },
            { value: '5k+', label: 'Pacientes felices' },
          ].map((s) => (
            <div key={s.label}>
              <p className="text-3xl sm:text-4xl font-extrabold bg-gradient-to-r from-sky-600 to-cyan-500 bg-clip-text text-transparent">
                {s.value}
              </p>
              <p className="mt-1 text-sm text-gray-500">{s.label}</p>
            </div>
          ))}
        </div>
      </section>

      {/* ═══════════════════════════ SERVICES GRID ═══════════════════════════ */}
      <section ref={servicesRef} className="py-20 sm:py-28">
        <div className="max-w-6xl mx-auto px-4 sm:px-6">
          <div className="text-center mb-14">
            <p className="text-sm font-semibold text-sky-600 uppercase tracking-wider mb-2">Lo que hacemos</p>
            <h2 className="text-3xl sm:text-4xl font-extrabold tracking-tight">Nuestros Servicios</h2>
            <p className="mt-4 text-gray-500 max-w-xl mx-auto">
              Ofrecemos tratamientos completos con tecnología de punta para cada miembro de tu familia.
            </p>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {clinica.especialidades.map((esp) => {
              const meta = SERVICE_META[esp] || DEFAULT_SERVICE;
              return (
                <div
                  key={esp}
                  className="group relative rounded-2xl border border-gray-100 bg-white p-7 hover:shadow-xl hover:shadow-sky-100/50 hover:-translate-y-1 transition-all duration-300"
                >
                  <div className="flex items-center justify-center w-14 h-14 rounded-xl bg-gradient-to-br from-sky-50 to-cyan-50 text-sky-600 mb-5 group-hover:scale-110 transition-transform">
                    {meta.icon}
                  </div>
                  <h3 className="text-lg font-bold mb-2">{esp}</h3>
                  <p className="text-sm text-gray-500 leading-relaxed">{meta.desc}</p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* ═══════════════════════════ TEAM ═══════════════════════════ */}
      <section ref={teamRef} className="py-20 sm:py-28 bg-gray-50/60">
        <div className="max-w-6xl mx-auto px-4 sm:px-6">
          <div className="text-center mb-14">
            <p className="text-sm font-semibold text-sky-600 uppercase tracking-wider mb-2">Profesionales</p>
            <h2 className="text-3xl sm:text-4xl font-extrabold tracking-tight">Nuestro Equipo</h2>
            <p className="mt-4 text-gray-500 max-w-xl mx-auto">
              Especialistas dedicados a brindarte la mejor atención dental posible.
            </p>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {clinica.dentistas.map((d) => (
              <div
                key={d.id}
                className="group rounded-2xl border border-gray-100 bg-white p-6 text-center hover:shadow-xl hover:shadow-sky-100/50 hover:-translate-y-1 transition-all duration-300"
              >
                <div className="mx-auto w-20 h-20 rounded-full bg-gradient-to-br from-sky-500 to-cyan-400 flex items-center justify-center text-white text-2xl font-bold shadow-lg shadow-sky-200/50 mb-4 group-hover:scale-110 transition-transform">
                  {d.nombre[0]}{d.apellido[0]}
                </div>
                <h3 className="text-lg font-bold">Dr. {d.nombre} {d.apellido}</h3>
                <p className="text-sm text-sky-600 font-medium mt-1">{d.especialidad || 'Odontología General'}</p>
                <button
                  onClick={() => { setShowBooking(true); setBooked(false); }}
                  className="mt-4 inline-flex items-center gap-1 text-sm font-medium text-gray-400 hover:text-sky-600 transition"
                >
                  Agendar cita <ArrowRight size={14} />
                </button>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ═══════════════════════════ LOCATION ═══════════════════════════ */}
      {clinica.direccion && (
        <section className="py-20 sm:py-28">
          <div className="max-w-6xl mx-auto px-4 sm:px-6">
            <div className="rounded-2xl border border-gray-100 bg-gradient-to-br from-sky-50/50 to-cyan-50/50 p-10 sm:p-14 flex flex-col sm:flex-row items-start sm:items-center gap-6">
              <div className="flex items-center justify-center w-16 h-16 rounded-2xl bg-white shadow-sm text-sky-600 shrink-0">
                <MapPin className="w-7 h-7" />
              </div>
              <div className="flex-1">
                <h3 className="text-xl font-bold mb-1">Encuéntranos</h3>
                <p className="text-gray-500">{clinica.direccion}</p>
                {clinica.telefono && (
                  <p className="text-gray-500 mt-1 flex items-center gap-1"><Phone size={14} /> {clinica.telefono}</p>
                )}
              </div>
              <a
                href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(clinica.direccion)}`}
                target="_blank"
                rel="noopener noreferrer"
                className="px-6 py-3 rounded-full bg-white border border-gray-200 text-sm font-semibold text-gray-700 hover:border-sky-300 hover:text-sky-600 transition shrink-0"
              >
                Ver en Maps
              </a>
            </div>
          </div>
        </section>
      )}

      {/* ═══════════════════════════ CTA BANNER ═══════════════════════════ */}
      <section className="py-20 sm:py-28">
        <div className="max-w-6xl mx-auto px-4 sm:px-6">
          <div className="relative rounded-3xl overflow-hidden bg-gradient-to-r from-sky-600 to-cyan-500 px-8 sm:px-16 py-14 sm:py-20 text-center text-white">
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_30%_50%,rgba(255,255,255,0.12),transparent_70%)]" />
            <h2 className="relative text-3xl sm:text-4xl font-extrabold tracking-tight mb-4">
              ¿Listo para transformar tu sonrisa?
            </h2>
            <p className="relative text-sky-100 max-w-xl mx-auto mb-8 text-lg">
              Agenda tu primera cita hoy y recibe una valoración completa sin costo.
            </p>
            <button
              onClick={() => { setShowBooking(true); setBooked(false); }}
              className="relative inline-flex items-center gap-2 px-8 py-4 rounded-full bg-white text-sky-700 font-bold shadow-xl hover:shadow-2xl hover:scale-105 transition-all"
            >
              <CalendarCheck size={18} />
              Agendar Cita Gratis
            </button>
          </div>
        </div>
      </section>

      {/* ═══════════════════════════ FOOTER ═══════════════════════════ */}
      <footer className="border-t border-gray-100">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 py-10 flex flex-col sm:flex-row items-center justify-between gap-4 text-sm text-gray-400">
          <div className="flex items-center gap-2">
            <span className="text-lg">🦷</span>
            <span className="font-semibold text-gray-500">{clinica.nombre}</span>
          </div>
          <p>&copy; {new Date().getFullYear()} {clinica.nombre}. Powered by <span className="font-semibold text-gray-500">DentiFlow</span></p>
        </div>
      </footer>

      {/* ═══════════════════════════ BOOKING MODAL ═══════════════════════════ */}
      {showBooking && (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-2xl max-w-lg w-full p-7 relative animate-in fade-in zoom-in">
            <button
              onClick={() => setShowBooking(false)}
              className="absolute top-4 right-4 w-8 h-8 flex items-center justify-center rounded-full bg-gray-100 text-gray-400 hover:text-gray-600 hover:bg-gray-200 transition"
            >
              <X size={16} />
            </button>

            {booked ? (
              <div className="text-center py-10">
                <div className="mx-auto w-16 h-16 rounded-full bg-green-100 flex items-center justify-center mb-5">
                  <CheckCircle size={32} className="text-green-500" />
                </div>
                <h3 className="text-2xl font-bold text-gray-900">¡Cita agendada!</h3>
                <p className="text-gray-500 mt-2">Te confirmaremos por correo o WhatsApp pronto.</p>
                <button
                  onClick={() => setShowBooking(false)}
                  className="mt-8 px-8 py-2.5 bg-gradient-to-r from-sky-600 to-cyan-500 text-white rounded-full font-semibold hover:shadow-lg transition"
                >
                  Cerrar
                </button>
              </div>
            ) : (
              <>
                <div className="mb-5">
                  <h3 className="text-xl font-bold text-gray-900">Agendar Cita</h3>
                  <p className="text-sm text-gray-400 mt-1">Completa tus datos y te contactaremos.</p>
                </div>
                <form onSubmit={handleBook} className="space-y-3">
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Nombre *</label>
                      <input type="text" name="nombre" required className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-sky-500/20 focus:border-sky-500 transition outline-none" />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Apellido *</label>
                      <input type="text" name="apellido" required className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-sky-500/20 focus:border-sky-500 transition outline-none" />
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Email</label>
                      <input type="email" name="email" className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-sky-500/20 focus:border-sky-500 transition outline-none" />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Teléfono</label>
                      <input type="tel" name="telefono" className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-sky-500/20 focus:border-sky-500 transition outline-none" />
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Especialista *</label>
                    <select name="dentistaId" required className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-sky-500/20 focus:border-sky-500 transition outline-none">
                      <option value="">Seleccionar especialista...</option>
                      {clinica.dentistas.map(d => (
                        <option key={d.id} value={d.id}>Dr. {d.nombre} {d.apellido} — {d.especialidad || 'General'}</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Fecha y Hora *</label>
                    <input type="datetime-local" name="fechaHora" required className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-sky-500/20 focus:border-sky-500 transition outline-none" />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Motivo</label>
                    <input type="text" name="motivo" placeholder="Ej. Limpieza dental, revisión general..." className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-sky-500/20 focus:border-sky-500 transition outline-none" />
                  </div>
                  <button
                    type="submit"
                    disabled={bookMutation.isPending}
                    className="w-full py-3 mt-2 bg-gradient-to-r from-sky-600 to-cyan-500 text-white font-semibold rounded-lg hover:shadow-lg hover:shadow-sky-500/25 transition-all disabled:opacity-50"
                  >
                    {bookMutation.isPending ? 'Agendando...' : 'Confirmar Cita'}
                  </button>
                  {bookMutation.isError && (
                    <p className="text-sm text-red-500 text-center">{(bookMutation.error as Error).message}</p>
                  )}
                </form>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

function LoadingScreen() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-white">
      <div className="relative w-14 h-14">
        <div className="absolute inset-0 rounded-full border-4 border-sky-100" />
        <div className="absolute inset-0 rounded-full border-4 border-transparent border-t-sky-500 animate-spin" />
      </div>
      <p className="mt-4 text-sm text-gray-400">Cargando clínica...</p>
    </div>
  );
}

function NotFoundScreen() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-white text-gray-500">
      <span className="text-7xl mb-6">🦷</span>
      <h2 className="text-3xl font-bold text-gray-900 mb-2">Clínica no encontrada</h2>
      <p className="text-gray-400">Verifica la URL e intenta de nuevo.</p>
    </div>
  );
}
