const db = require('../../config/database');

const UsuarioRepository = {
  async getPerfilCompleto(correo) {
    const [[row]] = await db.query(`
      SELECT
        u.*,
        m.nombre_membresia,
        m.precio                                                              AS membresia_precio,
        m.duracion_dias,
        t.nombre_tag,
        DATE_ADD(
          COALESCE(u.fecha_renovacion, u.fecha_inscripcion),
          INTERVAL m.duracion_dias DAY
        )                                                                     AS fecha_vencimiento,
        DATEDIFF(
          DATE_ADD(
            COALESCE(u.fecha_renovacion, u.fecha_inscripcion),
            INTERVAL m.duracion_dias DAY
          ),
          NOW()
        )                                                                     AS dias_restantes
      FROM  usuarios u
      JOIN  membresias m ON u.id_membresia = m.id_membresia
      LEFT JOIN tags   t ON u.id_tag       = t.id_tag
      WHERE u.correo = ?
      LIMIT 1
    `, [correo]);
    return row || null;
  },

  async countAsistencias(idUsuario) {
    const [[row]] = await db.query(
      'SELECT COUNT(*) AS total FROM acceso WHERE id_usuario = ?',
      [idUsuario]
    );
    return row?.total ?? 0;
  },

  async getClase(idClase) {
    const [[row]] = await db.query(
      'SELECT * FROM clases WHERE id_clase = ? LIMIT 1',
      [idClase]
    );
    return row || null;
  },

  async getRecords(idUsuario) {
    const [rows] = await db.query(
      'SELECT * FROM records_personales WHERE id_usuario = ? ORDER BY id_pr',
      [idUsuario]
    );
    return rows;
  },

  async findByCorreo(correo) {
    const [[row]] = await db.query(
      'SELECT * FROM usuarios WHERE correo = ? LIMIT 1',
      [correo]
    );
    return row || null;
  },

  async findWebByCorreo(correo) {
    const [[row]] = await db.query(
      'SELECT * FROM usuario_web WHERE correo_web = ? LIMIT 1',
      [correo]
    );
    return row || null;
  },

  async createUsuarioWeb(correo, telefono, googleId, foto) {
    const [result] = await db.query(
      `INSERT INTO usuario_web (correo_web, telefono, google_id, google_foto, tipo_auth)
       VALUES (?, ?, ?, ?, 'google')`,
      [correo, telefono, googleId, foto]
    );
    return result.insertId;
  },

  async linkUsuarioWeb(idUsuario, idUsuarioWeb) {
    await db.query(
      'UPDATE usuarios SET id_usuario_web = ? WHERE id_usuario = ?',
      [idUsuarioWeb, idUsuario]
    );
  },

  async updateGoogleCredentials(idUsuarioWeb, googleId, foto) {
    await db.query(
      'UPDATE usuario_web SET google_id = ?, google_foto = ? WHERE id_usuario_web = ?',
      [googleId, foto, idUsuarioWeb]
    );
  },
};

module.exports = UsuarioRepository;
