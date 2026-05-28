const PreRegistroRepository = require('../../infrastructure/repositories/PreRegistroRepository');
const MembresiaRepository   = require('../../infrastructure/repositories/MembresiaRepository');
const AppError              = require('../../domain/errors/AppError');

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_RE = /^\d{7,15}$/;

async function execute(body) {
  const { nombre_completo, edad, ciudad, correo, telefono, telefono_emergencia, id_membresia } = body;

  // ── Nombre ──────────────────────────────────────────────
  if (!nombre_completo?.trim() || nombre_completo.trim().length < 3)
    throw new AppError('El nombre debe tener al menos 3 caracteres.', 400);
  if (nombre_completo.trim().length > 100)
    throw new AppError('El nombre es demasiado largo (máx 100 caracteres).', 400);

  // ── Edad ────────────────────────────────────────────────
  if (edad !== undefined && edad !== null && edad !== '') {
    const edadNum = parseInt(edad, 10);
    if (isNaN(edadNum) || edadNum < 10 || edadNum > 100)
      throw new AppError('La edad debe estar entre 10 y 100 años.', 400);
  }

  // ── Ciudad ──────────────────────────────────────────────
  const ciudadFinal = (ciudad || 'Los Mochis').trim();
  if (ciudadFinal.length > 50)
    throw new AppError('La ciudad es demasiado larga (máx 50 caracteres).', 400);

  // ── Correo ──────────────────────────────────────────────
  if (correo && correo.trim()) {
    if (!EMAIL_RE.test(correo.trim()))
      throw new AppError('El correo electrónico no es válido.', 400);
    if (correo.trim().length > 100)
      throw new AppError('El correo es demasiado largo.', 400);
  }

  // ── Teléfonos ───────────────────────────────────────────
  if (telefono && !PHONE_RE.test(telefono.toString().trim()))
    throw new AppError('El teléfono debe contener solo dígitos (7-15 caracteres).', 400);
  if (telefono_emergencia && !PHONE_RE.test(telefono_emergencia.toString().trim()))
    throw new AppError('El teléfono de emergencia debe contener solo dígitos (7-15 caracteres).', 400);

  // ── id_membresia — verifica que exista en la BD ─────────
  let idMembresiaFinal = null;
  if (id_membresia !== undefined && id_membresia !== null && id_membresia !== '') {
    const idNum = parseInt(id_membresia, 10);
    if (isNaN(idNum) || idNum <= 0)
      throw new AppError('ID de membresía inválido.', 400);
    const existe = await MembresiaRepository.getById(idNum);
    if (!existe)
      throw new AppError('La membresía seleccionada no existe.', 400);
    idMembresiaFinal = idNum;
  }

  const expira_en = new Date(Date.now() + 60 * 1000);

  const id = await PreRegistroRepository.create({
    nombre_completo:    nombre_completo.trim(),
    edad:               edad ? parseInt(edad, 10) : null,
    ciudad:             ciudadFinal,
    correo:             correo?.trim() || null,
    telefono:           telefono?.toString().trim() || null,
    telefono_emergencia: telefono_emergencia?.toString().trim() || null,
    id_membresia:       idMembresiaFinal,
    expira_en,
  });

  return { id, expira_en };
}

module.exports = { execute };
