const API_BASE = '/api';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${url}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(err.error || 'Error de red');
  }
  return res.json();
}

// ── Types ──
export interface ClinicaProfile {
  id: string;
  nombre: string;
  slug: string;
  logoUrl: string | null;
  telefono: string | null;
  direccion: string | null;
  descripcion: string | null;
  especialidades: string[];
  dentistas: DentistaResumen[];
}

export interface DentistaResumen {
  id: string;
  nombre: string;
  apellido: string;
  especialidad: string | null;
}

export interface Dentista {
  id: string;
  clinicaId: string;
  nombre: string;
  apellido: string;
  email: string;
  especialidad: string | null;
  telefono: string | null;
  googleCalendarConnected: boolean;
  googleCalendarEmail: string | null;
  createdAt: string;
}

export interface Paciente {
  id: string;
  clinicaId: string;
  nombre: string;
  apellido: string;
  email: string | null;
  telefono: string | null;
  notas: string | null;
  createdAt: string;
}

export interface Cita {
  id: string;
  fechaHora: string;
  duracionMinutos: number;
  motivo: string | null;
  estado: string;
  nombreDentista: string;
  nombrePaciente: string;
  createdAt: string;
}

// ── Clinica ──
export const getClinicaProfile = (slug: string) =>
  request<ClinicaProfile>(`/clinica/${slug}`);

// ── Dentistas ──
export const getDentistas = (clinicaId: string) =>
  request<Dentista[]>(`/dentistas?clinicaId=${clinicaId}`);

export const createDentista = (data: {
  clinicaId: string;
  nombre: string;
  apellido: string;
  email: string;
  especialidad?: string;
  telefono?: string;
}) => request<Dentista>('/dentistas', { method: 'POST', body: JSON.stringify(data) });

// ── Pacientes ──
export const getPacientes = (clinicaId: string) =>
  request<Paciente[]>(`/pacientes?clinicaId=${clinicaId}`);

export const createPaciente = (data: {
  clinicaId: string;
  nombre: string;
  apellido: string;
  email?: string;
  telefono?: string;
  notas?: string;
}) => request<Paciente>('/pacientes', { method: 'POST', body: JSON.stringify(data) });

export const updatePaciente = (id: string, data: {
  nombre: string;
  apellido: string;
  email?: string;
  telefono?: string;
  notas?: string;
}) => request<Paciente>(`/pacientes/${id}`, { method: 'PUT', body: JSON.stringify(data) });

// ── Citas ──
export const getAppointments = (clinicaId: string, desde: string, hasta: string) =>
  request<Cita[]>(`/appointments?clinicaId=${clinicaId}&desde=${desde}&hasta=${hasta}`);

export const bookAppointment = (data: {
  clinicaId: string;
  dentistaId: string;
  nombrePaciente: string;
  apellidoPaciente: string;
  emailPaciente?: string;
  telefonoPaciente?: string;
  fechaHora: string;
  duracionMinutos?: number;
  motivo?: string;
}) => request<Cita>('/appointments/book', { method: 'POST', body: JSON.stringify(data) });

export const updateAppointmentStatus = (id: string, estado: string) =>
  request<Cita>(`/appointments/${id}/status`, {
    method: 'PATCH',
    body: JSON.stringify({ estado }),
  });

export const cancelAppointment = (id: string) =>
  request<Cita>(`/appointments/${id}`, { method: 'DELETE' });

// ── Google Calendar ──
export interface GoogleCalendarStatus {
  connected: boolean;
  googleEmail: string | null;
  tokenExpiry: string | null;
}

export const getGoogleCalendarAuthUrl = (dentistaId: string) =>
  request<{ authUrl: string }>(`/google-calendar/auth-url?dentistaId=${dentistaId}`);

export const getGoogleCalendarStatus = (dentistaId: string) =>
  request<GoogleCalendarStatus>(`/google-calendar/status/${dentistaId}`);

export const disconnectGoogleCalendar = (dentistaId: string) =>
  request<{ message: string }>(`/google-calendar/disconnect/${dentistaId}`, { method: 'DELETE' });
