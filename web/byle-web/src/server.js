require('dotenv').config();

// ── Validación de variables de entorno críticas ──────────
if (!process.env.SESSION_SECRET) {
  console.error('[FATAL] SESSION_SECRET no está definido en .env');
  process.exit(1);
}
if (!process.env.DB_HOST || !process.env.DB_USER || !process.env.DB_PASSWORD || !process.env.DB_NAME) {
  console.error('[FATAL] Variables de base de datos (DB_HOST, DB_USER, DB_PASSWORD, DB_NAME) no están definidas.');
  process.exit(1);
}

const app                 = require('./app');
const { startCleanupJob } = require('./application/usecases/CleanExpiredPreRegistrosUseCase');

const PORT = process.env.PORT || 3000;

startCleanupJob();

app.listen(PORT, () => {
  console.log(`Byle Web v2 → http://localhost:${PORT}`);
});
