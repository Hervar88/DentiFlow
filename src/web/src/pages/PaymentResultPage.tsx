import { useSearchParams, Link } from 'react-router-dom';
import { CheckCircle, XCircle, Clock } from 'lucide-react';

type ResultType = 'success' | 'failure' | 'pending';

const config: Record<ResultType, { icon: React.ReactNode; title: string; description: string; color: string }> = {
  success: {
    icon: <CheckCircle size={48} className="text-green-500" />,
    title: '¡Pago exitoso!',
    description: 'Tu anticipo ha sido procesado correctamente. Tu cita ha sido confirmada.',
    color: 'bg-green-50 border-green-200',
  },
  failure: {
    icon: <XCircle size={48} className="text-red-500" />,
    title: 'Pago no completado',
    description: 'Hubo un problema con tu pago. Puedes intentarlo de nuevo desde el enlace que te enviamos.',
    color: 'bg-red-50 border-red-200',
  },
  pending: {
    icon: <Clock size={48} className="text-amber-500" />,
    title: 'Pago pendiente',
    description: 'Tu pago está siendo procesado. Te notificaremos cuando se confirme.',
    color: 'bg-amber-50 border-amber-200',
  },
};

export default function PaymentResultPage({ type }: { type: ResultType }) {
  const [searchParams] = useSearchParams();
  const citaId = searchParams.get('citaId');
  const { icon, title, description, color } = config[type];

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <div className={`max-w-md w-full rounded-2xl border ${color} p-8 text-center shadow-lg`}>
        <div className="flex justify-center mb-5">{icon}</div>
        <h1 className="text-2xl font-bold text-gray-900 mb-2">{title}</h1>
        <p className="text-gray-600 mb-6">{description}</p>
        {citaId && (
          <p className="text-xs text-gray-400 mb-4">
            Referencia: {citaId}
          </p>
        )}
        <Link
          to="/"
          className="inline-flex items-center gap-2 px-6 py-3 rounded-full bg-gradient-to-r from-sky-600 to-cyan-500 text-white font-semibold shadow-lg hover:shadow-xl transition-all hover:scale-105"
        >
          Volver al inicio
        </Link>
      </div>
    </div>
  );
}
