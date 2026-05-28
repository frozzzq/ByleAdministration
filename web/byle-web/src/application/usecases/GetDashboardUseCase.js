const UsuarioRepository = require('../../infrastructure/repositories/UsuarioRepository');

const fmt = d => d
  ? new Date(d).toLocaleDateString('es-MX', { day: 'numeric', month: 'short', year: 'numeric' })
  : '—';

async function execute(correo) {
  const perfil = await UsuarioRepository.getPerfilCompleto(correo);
  if (!perfil) return null;

  const [totalAsistencias, clase, records] = await Promise.all([
    UsuarioRepository.countAsistencias(perfil.id_usuario),
    perfil.id_clase
      ? UsuarioRepository.getClase(perfil.id_clase)
      : Promise.resolve(null),
    UsuarioRepository.getRecords(perfil.id_usuario),
  ]);

  return {
    perfil: {
      ...perfil,
      fecha_inscripcion_fmt: fmt(perfil.fecha_inscripcion),
      fecha_renovacion_fmt:  fmt(perfil.fecha_renovacion),
      fecha_vencimiento_fmt: fmt(perfil.fecha_vencimiento),
    },
    total_asistencias: totalAsistencias,
    clase,
    records,
  };
}

module.exports = { execute };
