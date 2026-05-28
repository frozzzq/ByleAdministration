const db = require('../../config/database');

const MembresiaRepository = {
  async getAll() {
    const [rows] = await db.query(
      'SELECT * FROM membresias ORDER BY precio ASC'
    );
    return rows;
  },

  async getActivas() {
    const [rows] = await db.query(
      `SELECT id_membresia, nombre_membresia, precio, duracion_dias, descripcion, estado
       FROM   membresias
       WHERE  estado IN ('activa', 'oferta', 'temporada')
       ORDER  BY precio ASC`
    );
    return rows;
  },
};

module.exports = MembresiaRepository;
