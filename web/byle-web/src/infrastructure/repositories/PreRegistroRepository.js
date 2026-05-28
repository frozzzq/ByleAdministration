const db = require('../../config/database');

const PreRegistroRepository = {
  async create({ nombre_completo, edad, ciudad, correo, telefono, telefono_emergencia, id_membresia, expira_en }) {
    const [result] = await db.query(
      `INSERT INTO pre_registros
         (nombre_completo, edad, ciudad, correo, telefono, telefono_emergencia, id_membresia, expira_en)
       VALUES (?, ?, ?, ?, ?, ?, ?, ?)`,
      [
        nombre_completo,
        edad          || null,
        ciudad,
        correo        || null,
        telefono      || null,
        telefono_emergencia || null,
        id_membresia  || null,
        expira_en,
      ]
    );
    return result.insertId;
  },

  async cleanExpired() {
    await db.query(
      `UPDATE pre_registros SET estado = 'expirado'
       WHERE  estado = 'pendiente' AND expira_en < NOW()`
    );
  },
};

module.exports = PreRegistroRepository;
