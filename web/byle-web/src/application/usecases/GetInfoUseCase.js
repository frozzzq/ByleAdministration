const MembresiaRepository = require('../../infrastructure/repositories/MembresiaRepository');
const ClaseRepository     = require('../../infrastructure/repositories/ClaseRepository');

const SECCIONES_VALIDAS = new Set(['general', 'planes', 'horarios']);

async function execute(seccion) {
  // Normalizar y validar contra whitelist — previene valores arbitrarios en la vista
  const raw        = seccion === 'instalaciones' ? 'general' : (seccion || 'general');
  const normalized = SECCIONES_VALIDAS.has(raw) ? raw : 'general';

  let membresias = [];
  let clases     = [];

  if (normalized === 'planes') {
    membresias = await MembresiaRepository.getActivas();
  } else if (normalized === 'horarios') {
    clases = await ClaseRepository.getAll();
  }

  return { seccion: normalized, membresias, clases };
}

module.exports = { execute };
