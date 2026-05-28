require('dotenv').config();

const app              = require('./app');
const { startCleanupJob } = require('./application/usecases/CleanExpiredPreRegistrosUseCase');

const PORT = process.env.PORT || 3000;

startCleanupJob();

app.listen(PORT, () => {
  console.log(`Byle Web v2 → http://localhost:${PORT}`);
});
