const MembresiaRepository = require('../../infrastructure/repositories/MembresiaRepository');
const ClaseRepository     = require('../../infrastructure/repositories/ClaseRepository');

async function execute(seccion) {
  const normalized = (seccion === 'instalaciones' ? 'general' : seccion) || 'general';
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
