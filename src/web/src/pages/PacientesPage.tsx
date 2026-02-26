import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getClinicaProfile, getPacientes, createPaciente, updatePaciente, type Paciente } from '../api';
import { CLINICA_SLUG } from '../App';
import { Plus, X, Pencil, Search } from 'lucide-react';

export default function PacientesPage() {
  const queryClient = useQueryClient();
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  const { data: clinica } = useQuery({
    queryKey: ['clinica', CLINICA_SLUG],
    queryFn: () => getClinicaProfile(CLINICA_SLUG),
  });

  const { data: pacientes = [] } = useQuery({
    queryKey: ['pacientes', clinica?.id],
    queryFn: () => getPacientes(clinica!.id),
    enabled: !!clinica?.id,
  });

  const createMutation = useMutation({
    mutationFn: createPaciente,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pacientes'] });
      setShowForm(false);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, ...data }: { id: string; nombre: string; apellido: string; email?: string; telefono?: string; notas?: string }) =>
      updatePaciente(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pacientes'] });
      setEditingId(null);
    },
  });

  const handleCreate = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    createMutation.mutate({
      clinicaId: clinica!.id,
      nombre: fd.get('nombre') as string,
      apellido: fd.get('apellido') as string,
      email: (fd.get('email') as string) || undefined,
      telefono: (fd.get('telefono') as string) || undefined,
      notas: (fd.get('notas') as string) || undefined,
    });
  };

  const handleUpdate = (e: React.FormEvent<HTMLFormElement>, id: string) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    updateMutation.mutate({
      id,
      nombre: fd.get('nombre') as string,
      apellido: fd.get('apellido') as string,
      email: (fd.get('email') as string) || undefined,
      telefono: (fd.get('telefono') as string) || undefined,
      notas: (fd.get('notas') as string) || undefined,
    });
  };

  const filtered = pacientes.filter(p => {
    const q = search.toLowerCase();
    return `${p.nombre} ${p.apellido} ${p.email ?? ''} ${p.telefono ?? ''}`.toLowerCase().includes(q);
  });

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Pacientes</h1>
        <button
          onClick={() => { setShowForm(!showForm); setEditingId(null); }}
          className="flex items-center gap-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition text-sm font-medium"
        >
          {showForm ? <X size={16} /> : <Plus size={16} />}
          {showForm ? 'Cancelar' : 'Nuevo Paciente'}
        </button>
      </div>

      {/* Create form */}
      {showForm && (
        <form onSubmit={handleCreate} className="bg-white rounded-xl border shadow-sm p-6 mb-6 grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Nombre *</label>
            <input type="text" name="nombre" required className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Apellido *</label>
            <input type="text" name="apellido" required className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input type="email" name="email" className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Teléfono</label>
            <input type="tel" name="telefono" className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">Notas</label>
            <textarea name="notas" rows={2} className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div className="md:col-span-2">
            <button type="submit" disabled={createMutation.isPending} className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition text-sm font-medium disabled:opacity-50">
              {createMutation.isPending ? 'Guardando...' : 'Guardar Paciente'}
            </button>
          </div>
        </form>
      )}

      {/* Search */}
      <div className="relative mb-4">
        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
        <input
          type="text"
          placeholder="Buscar paciente..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-full pl-9 pr-4 py-2 border rounded-lg text-sm"
        />
      </div>

      {/* Patients Table */}
      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Nombre</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Email</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Teléfono</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Notas</th>
              <th className="text-left px-4 py-3 font-medium text-gray-500">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {filtered.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-4 py-12 text-center text-gray-400">
                  {search ? 'No se encontraron pacientes.' : 'Aún no hay pacientes registrados.'}
                </td>
              </tr>
            ) : (
              filtered.map((p) =>
                editingId === p.id ? (
                  <EditRow key={p.id} paciente={p} onSubmit={(e) => handleUpdate(e, p.id)} onCancel={() => setEditingId(null)} isPending={updateMutation.isPending} />
                ) : (
                  <PacienteRow key={p.id} paciente={p} onEdit={() => { setEditingId(p.id); setShowForm(false); }} />
                )
              )
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function PacienteRow({ paciente: p, onEdit }: { paciente: Paciente; onEdit: () => void }) {
  return (
    <tr className="hover:bg-gray-50">
      <td className="px-4 py-3 font-medium">{p.nombre} {p.apellido}</td>
      <td className="px-4 py-3 text-gray-500">{p.email || '—'}</td>
      <td className="px-4 py-3 text-gray-500">{p.telefono || '—'}</td>
      <td className="px-4 py-3 text-gray-500 max-w-[200px] truncate">{p.notas || '—'}</td>
      <td className="px-4 py-3">
        <button onClick={onEdit} className="text-blue-600 hover:text-blue-800">
          <Pencil size={14} />
        </button>
      </td>
    </tr>
  );
}

function EditRow({ paciente: p, onSubmit, onCancel, isPending }: { paciente: Paciente; onSubmit: (e: React.FormEvent<HTMLFormElement>) => void; onCancel: () => void; isPending: boolean }) {
  return (
    <tr>
      <td colSpan={5} className="px-4 py-3">
        <form onSubmit={onSubmit} className="grid grid-cols-1 md:grid-cols-5 gap-2 items-end">
          <input name="nombre" defaultValue={p.nombre} required className="border rounded px-2 py-1 text-sm" placeholder="Nombre" />
          <input name="apellido" defaultValue={p.apellido} required className="border rounded px-2 py-1 text-sm" placeholder="Apellido" />
          <input name="email" defaultValue={p.email ?? ''} className="border rounded px-2 py-1 text-sm" placeholder="Email" />
          <input name="telefono" defaultValue={p.telefono ?? ''} className="border rounded px-2 py-1 text-sm" placeholder="Teléfono" />
          <div className="flex gap-1">
            <input name="notas" defaultValue={p.notas ?? ''} className="border rounded px-2 py-1 text-sm flex-1" placeholder="Notas" />
            <button type="submit" disabled={isPending} className="px-2 py-1 bg-blue-600 text-white rounded text-xs">Guardar</button>
            <button type="button" onClick={onCancel} className="px-2 py-1 bg-gray-200 text-gray-700 rounded text-xs">Cancelar</button>
          </div>
        </form>
      </td>
    </tr>
  );
}
