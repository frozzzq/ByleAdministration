const rateLimit = require('express-rate-limit');

// Límite estricto para el endpoint de pre-registro (anti-spam/DoS)
const preRegistroLimiter = rateLimit({
  windowMs:             15 * 60 * 1000, // 15 minutos
  max:                  5,              // máx 5 solicitudes por IP por ventana
  standardHeaders:      true,
  legacyHeaders:        false,
  message:              { ok: false, error: 'Demasiadas solicitudes. Intenta de nuevo en 15 minutos.' },
  skipSuccessfulRequests: false,
});

// Límite para rutas de autenticación (anti-fuerza-bruta en OAuth)
const authLimiter = rateLimit({
  windowMs:        60 * 60 * 1000, // 1 hora
  max:             20,
  standardHeaders: true,
  legacyHeaders:   false,
  message:         'Demasiados intentos de autenticación. Intenta de nuevo en una hora.',
});

module.exports = { preRegistroLimiter, authLimiter };
