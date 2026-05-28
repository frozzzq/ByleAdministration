const db = require('../../config/database');

const ClaseRepository = {
  async getAll() {
    const [rows] = await db.query('SELECT * FROM clases ORDER BY costo ASC');
    return rows;
  },
};

module.exports = ClaseRepository;
