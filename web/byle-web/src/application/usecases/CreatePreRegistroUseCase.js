const PreRegistroRepository = require('../../infrastructure/repositories/PreRegistroRepository');
const AppError              = require('../../domain/errors/AppError');

async function execute(body) {
  const { nombre_completo, edad, ciudad, correo, telefono, telefono_emergencia, id_membresia } = body;

  if (!nombre_completo?.trim()) {
    throw new AppError('El nombre es obligatorio.', 400);
  }

  const expira_en = new Date(Date.now() + 60 * 1000);

  const id = await PreRegistroRepository.create({
    nombre_completo:    nombre_completo.trim(),
    edad:               edad || null,
    ciudad:             (ciudad || 'Los Mochis').trim(),
    correo:             correo || null,
    telefono:           telefono || null,
    telefono_emergencia: telefono_emergencia || null,
    id_membresia:       id_membresia || null,
    expira_en,
  });

  return { id, expira_en };
}

module.exports = { execute };
