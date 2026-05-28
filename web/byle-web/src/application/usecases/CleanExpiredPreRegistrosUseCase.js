const PreRegistroRepository = require('../../infrastructure/repositories/PreRegistroRepository');

async function execute() {
  await PreRegistroRepository.cleanExpired();
}

function startCleanupJob() {
  setInterval(async () => {
    try {
      await execute();
    } catch (err) {
      console.error('Cleanup pre_registros error:', err);
    }
  }, 60_000);
}

module.exports = { execute, startCleanupJob };
