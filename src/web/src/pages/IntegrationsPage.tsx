import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getDentistas, getGoogleCalendarAuthUrl, getGoogleCalendarStatus, disconnectGoogleCalendar, isPaymentConfigured, isWhatsAppConfigured, type Dentista } from '../api';
import { CLINICA_SLUG } from '../App';
import { getClinicaProfile } from '../api';
import { Calendar, Link2, Unlink, CheckCircle2, Loader2, CreditCard, AlertTriangle, MessageCircle } from 'lucide-react';
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

      {/* Mercado Pago Section */}
      <MercadoPagoCard />

      {/* WhatsApp Section */}
      <WhatsAppCard />
    </div>
  );
}

function MercadoPagoCard() {
  const { data: paymentConfig, isLoading } = useQuery({
    queryKey: ['payment-configured'],
    queryFn: isPaymentConfigured,
  });

  const configured = paymentConfig?.configured ?? false;

  return (
    <div className="bg-white rounded-xl border shadow-sm mb-6">
      <div className="px-6 py-4 border-b flex items-center gap-3">
        <div className="p-2 bg-sky-50 rounded-lg">
          <CreditCard className="text-sky-600" size={22} />
        </div>
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <h2 className="text-lg font-semibold text-gray-900">Mercado Pago</h2>
            {isLoading ? (
              <Loader2 size={14} className="animate-spin text-gray-400" />
            ) : configured ? (
              <span className="text-xs bg-green-50 text-green-700 px-2 py-0.5 rounded-full font-medium flex items-center gap-1">
                <CheckCircle2 size={12} /> Configurado
              </span>
            ) : (
              <span className="text-xs bg-amber-50 text-amber-700 px-2 py-0.5 rounded-full font-medium flex items-center gap-1">
                <AlertTriangle size={12} /> Sin configurar
              </span>
            )}
          </div>
          <p className="text-sm text-gray-500">
            Cobro de anticipos con Checkout Pro para confirmar citas.
          </p>
        </div>
      </div>

      <div className="p-6">
        {configured ? (
          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
            <p className="text-sm text-green-800 font-medium">Mercado Pago está activo</p>
            <p className="text-sm text-green-600 mt-1">
              Los pacientes pueden pagar anticipos desde la tabla de citas con el botón "Cobrar".
              El monto del anticipo está configurado en el servidor.
            </p>
          </div>
        ) : (
          <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
            <p className="text-sm text-amber-800 font-medium">Configuración pendiente</p>
            <p className="text-sm text-amber-600 mt-1">
              Para activar los cobros, agrega tu <strong>Access Token</strong> de Mercado Pago en la
              configuración del servidor (<code>appsettings.json</code> → sección <code>MercadoPago</code>).
            </p>
            <a
              href="https://www.mercadopago.com.mx/developers/panel/app"
              target="_blank"
              rel="noopener noreferrer"
              className="inline-block mt-3 text-sm font-medium text-sky-600 hover:text-sky-700 underline"
            >
              Ir al panel de desarrolladores de Mercado Pago →
            </a>
          </div>
        )}
      </div>

      <div className="px-6 py-4 bg-gray-50 border-t rounded-b-xl">
        <h3 className="text-sm font-medium text-gray-700 mb-2">¿Cómo funciona?</h3>
        <ul className="text-sm text-gray-500 space-y-1">
          <li>• Se genera un link de pago (Checkout Pro) al hacer clic en "Cobrar" en una cita</li>
          <li>• El paciente paga el anticipo desde su navegador con tarjeta, transferencia u OXXO</li>
          <li>• Al confirmarse el pago, la cita cambia automáticamente a estado "Pagada"</li>
          <li>• El monto del anticipo se configura en el servidor (default: $500 MXN)</li>
        </ul>
      </div>
    </div>
  );
}

function WhatsAppCard() {
  const { data: waConfig, isLoading } = useQuery({
    queryKey: ['whatsapp-configured'],
    queryFn: isWhatsAppConfigured,
  });

  const configured = waConfig?.configured ?? false;

  return (
    <div className="bg-white rounded-xl border shadow-sm mb-6">
      <div className="px-6 py-4 border-b flex items-center gap-3">
        <div className="p-2 bg-green-50 rounded-lg">
          <MessageCircle className="text-green-600" size={22} />
        </div>
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <h2 className="text-lg font-semibold text-gray-900">WhatsApp</h2>
            {isLoading ? (
              <Loader2 size={14} className="animate-spin text-gray-400" />
            ) : configured ? (
              <span className="text-xs bg-green-50 text-green-700 px-2 py-0.5 rounded-full font-medium flex items-center gap-1">
                <CheckCircle2 size={12} /> Configurado
              </span>
            ) : (
              <span className="text-xs bg-amber-50 text-amber-700 px-2 py-0.5 rounded-full font-medium flex items-center gap-1">
                <AlertTriangle size={12} /> Sin configurar
              </span>
            )}
          </div>
          <p className="text-sm text-gray-500">
            Notificaciones automáticas por WhatsApp a pacientes vía Twilio.
          </p>
        </div>
      </div>

      <div className="p-6">
        {configured ? (
          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
            <p className="text-sm text-green-800 font-medium">WhatsApp está activo</p>
            <p className="text-sm text-green-600 mt-1">
              Los pacientes recibirán notificaciones automáticas al agendar, cancelar o cambiar
              el estado de sus citas. También puedes enviar recordatorios manuales desde la tabla de citas.
            </p>
          </div>
        ) : (
          <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
            <p className="text-sm text-amber-800 font-medium">Configuración pendiente</p>
            <p className="text-sm text-amber-600 mt-1">
              Para activar WhatsApp, agrega tus credenciales de <strong>Twilio</strong> en la
              configuración del servidor (<code>appsettings.json</code> → sección <code>Twilio</code>):
              Account SID, Auth Token y número de WhatsApp.
            </p>
            <a
              href="https://www.twilio.com/console/sms/whatsapp/sandbox"
              target="_blank"
              rel="noopener noreferrer"
              className="inline-block mt-3 text-sm font-medium text-sky-600 hover:text-sky-700 underline"
            >
              Ir al sandbox de WhatsApp en Twilio →
            </a>
          </div>
        )}
      </div>

      <div className="px-6 py-4 bg-gray-50 border-t rounded-b-xl">
        <h3 className="text-sm font-medium text-gray-700 mb-2">¿Cómo funciona?</h3>
        <ul className="text-sm text-gray-500 space-y-1">
          <li>• Al agendar una cita, el paciente recibe confirmación por WhatsApp</li>
          <li>• Si se cancela una cita, se notifica automáticamente al paciente</li>
          <li>• Los cambios de estado (confirmada, pagada, completada) también generan notificación</li>
          <li>• Puedes enviar recordatorios manuales antes de las citas</li>
          <li>• El número del paciente debe incluir código de país (ej: +52 para México)</li>
        </ul>
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
