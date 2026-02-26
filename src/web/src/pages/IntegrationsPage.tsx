import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getDentistas, getGoogleCalendarAuthUrl, getGoogleCalendarStatus, disconnectGoogleCalendar, type Dentista } from '../api';
import { CLINICA_SLUG } from '../App';
import { getClinicaProfile } from '../api';
import { Calendar, Link2, Unlink, CheckCircle2, Loader2 } from 'lucide-react';
import { useState } from 'react';

export default function IntegrationsPage() {
  const queryClient = useQueryClient();

  const { data: clinica } = useQuery({
    queryKey: ['clinica', CLINICA_SLUG],
    queryFn: () => getClinicaProfile(CLINICA_SLUG),
  });

  const { data: dentistas = [] } = useQuery({
    queryKey: ['dentistas', clinica?.id],
    queryFn: () => getDentistas(clinica!.id),
    enabled: !!clinica?.id,
  });

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-2">Integraciones</h1>
      <p className="text-gray-500 mb-8">Conecta servicios externos para sincronizar tu agenda.</p>

      {/* Google Calendar Section */}
      <div className="bg-white rounded-xl border shadow-sm mb-6">
        <div className="px-6 py-4 border-b flex items-center gap-3">
          <div className="p-2 bg-blue-50 rounded-lg">
            <Calendar className="text-blue-600" size={22} />
          </div>
          <div>
            <h2 className="text-lg font-semibold text-gray-900">Google Calendar</h2>
            <p className="text-sm text-gray-500">
              Sincroniza citas automáticamente con el calendario de cada dentista.
            </p>
          </div>
        </div>

        <div className="p-6">
          {dentistas.length === 0 ? (
            <p className="text-gray-400 text-center py-8">No hay dentistas registrados.</p>
          ) : (
            <div className="space-y-4">
              {dentistas.map((dentista) => (
                <DentistaCalendarCard
                  key={dentista.id}
                  dentista={dentista}
                  onStatusChange={() => {
                    queryClient.invalidateQueries({ queryKey: ['dentistas'] });
                  }}
                />
              ))}
            </div>
          )}
        </div>

        {/* How it works */}
        <div className="px-6 py-4 bg-gray-50 border-t rounded-b-xl">
          <h3 className="text-sm font-medium text-gray-700 mb-2">¿Cómo funciona?</h3>
          <ul className="text-sm text-gray-500 space-y-1">
            <li>• Al vincular, las nuevas citas aparecen automáticamente en Google Calendar</li>
            <li>• Si mueves una cita en Google Calendar, se actualiza en DentiFlow</li>
            <li>• Si cancelas en DentiFlow, se elimina del calendario</li>
            <li>• Cada evento incluye datos del paciente y motivo de la cita</li>
          </ul>
        </div>
      </div>

      {/* Placeholder for future integrations */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <IntegrationPlaceholder
          title="Mercado Pago"
          description="Cobro de anticipos con Checkout Pro"
          status="Próximamente"
        />
        <IntegrationPlaceholder
          title="WhatsApp"
          description="Recordatorios automáticos a pacientes"
          status="Próximamente"
        />
      </div>
    </div>
  );
}

function DentistaCalendarCard({ dentista, onStatusChange }: {
  dentista: Dentista;
  onStatusChange: () => void;
}) {
  const [loading, setLoading] = useState(false);
  const queryClient = useQueryClient();

  const { data: status } = useQuery({
    queryKey: ['gcal-status', dentista.id],
    queryFn: () => getGoogleCalendarStatus(dentista.id),
    refetchInterval: dentista.googleCalendarConnected ? 60_000 : false,
  });

  const connected = status?.connected ?? dentista.googleCalendarConnected;

  const handleConnect = async () => {
    setLoading(true);
    try {
      const { authUrl } = await getGoogleCalendarAuthUrl(dentista.id);
      window.location.href = authUrl;
    } catch {
      setLoading(false);
    }
  };

  const disconnectMutation = useMutation({
    mutationFn: () => disconnectGoogleCalendar(dentista.id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['gcal-status', dentista.id] });
      onStatusChange();
    },
  });

  return (
    <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg border">
      <div className="flex items-center gap-3">
        <div className={`w-2 h-2 rounded-full ${connected ? 'bg-green-500' : 'bg-gray-300'}`} />
        <div>
          <p className="text-sm font-medium text-gray-900">
            Dr. {dentista.nombre} {dentista.apellido}
          </p>
          <p className="text-xs text-gray-500">
            {dentista.especialidad ?? 'General'}
          </p>
          {connected && status?.googleEmail && (
            <p className="text-xs text-green-600 flex items-center gap-1 mt-0.5">
              <CheckCircle2 size={12} />
              {status.googleEmail}
            </p>
          )}
        </div>
      </div>

      <div>
        {connected ? (
          <button
            onClick={() => disconnectMutation.mutate()}
            disabled={disconnectMutation.isPending}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm text-red-600 bg-red-50 hover:bg-red-100 border border-red-200 rounded-lg transition"
          >
            {disconnectMutation.isPending ? (
              <Loader2 size={14} className="animate-spin" />
            ) : (
              <Unlink size={14} />
            )}
            Desconectar
          </button>
        ) : (
          <button
            onClick={handleConnect}
            disabled={loading}
            className="flex items-center gap-1.5 px-3 py-1.5 text-sm text-blue-600 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-lg transition"
          >
            {loading ? (
              <Loader2 size={14} className="animate-spin" />
            ) : (
              <Link2 size={14} />
            )}
            Vincular Calendar
          </button>
        )}
      </div>
    </div>
  );
}

function IntegrationPlaceholder({ title, description, status }: {
  title: string;
  description: string;
  status: string;
}) {
  return (
    <div className="bg-white rounded-xl border shadow-sm p-5 opacity-60">
      <div className="flex items-center justify-between mb-2">
        <h3 className="font-semibold text-gray-900">{title}</h3>
        <span className="text-xs bg-gray-100 text-gray-500 px-2 py-0.5 rounded-full">{status}</span>
      </div>
      <p className="text-sm text-gray-500">{description}</p>
    </div>
  );
}
