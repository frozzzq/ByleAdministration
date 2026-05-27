require('dotenv').config();
const express  = require('express');
const path     = require('path');
const session  = require('express-session');
const passport = require('passport');
const { Strategy: GoogleStrategy } = require('passport-google-oauth20');
const mysql    = require('mysql2/promise');

const app  = express();
const PORT = process.env.PORT || 3000;

// ── MySQL Pool ──────────────────────────────────────────
const db = mysql.createPool({
  host:            process.env.DB_HOST,
  user:            process.env.DB_USER,
  password:        process.env.DB_PASSWORD,
  database:        process.env.DB_NAME,
  waitForConnections: true,
  connectionLimit: 10,
});

// ── Motor de vistas ──────────────────────────────────────
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));
app.use(express.static(path.join(__dirname, 'public')));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// ── Sesiones ────────────────────────────────────────────
app.use(session({
  secret:           process.env.SESSION_SECRET || 'byle-secret',
  resave:           false,
  saveUninitialized: false,
  cookie: { httpOnly: true, secure: false, maxAge: 7 * 24 * 60 * 60 * 1000 },
}));

// ── Passport: Google OAuth ───────────────────────────────
passport.use(new GoogleStrategy(
  {
    clientID:     process.env.GOOGLE_CLIENT_ID,
    clientSecret: process.env.GOOGLE_CLIENT_SECRET,
    callbackURL:  `${process.env.BASE_URL || 'http://localhost:3000'}/auth/google/callback`,
  },
  async (accessToken, refreshToken, profile, done) => {
    try {
      const correo   = profile.emails[0].value;
      const googleId = profile.id;
      const foto     = profile.photos?.[0]?.value || null;

      // 1. Verificar que el correo esté registrado como socio en la tabla usuarios
      const [[usuario]] = await db.query(
        'SELECT * FROM usuarios WHERE correo = ? LIMIT 1',
        [correo]
      );
      if (!usuario) {
        return done(null, false, { message: 'no_registrado' });
      }

      // 2. Buscar o crear el registro en usuario_web
      let [[usuarioWeb]] = await db.query(
        'SELECT * FROM usuario_web WHERE correo_web = ? LIMIT 1',
        [correo]
      );

      if (!usuarioWeb) {
        const [ins] = await db.query(
          `INSERT INTO usuario_web (correo_web, telefono, google_id, google_foto, tipo_auth)
           VALUES (?, ?, ?, ?, 'google')`,
          [correo, usuario.telefono, googleId, foto]
        );
        usuarioWeb = { id_usuario_web: ins.insertId };

        // Vincular con el registro de usuarios si aún no está vinculado
        if (!usuario.id_usuario_web) {
          await db.query(
            'UPDATE usuarios SET id_usuario_web = ? WHERE id_usuario = ?',
            [ins.insertId, usuario.id_usuario]
          );
        }
      } else {
        // Actualizar google_id y foto por si el usuario cambió de cuenta
        await db.query(
          'UPDATE usuario_web SET google_id = ?, google_foto = ? WHERE id_usuario_web = ?',
          [googleId, foto, usuarioWeb.id_usuario_web]
        );
      }

      return done(null, {
        id_usuario_web: usuarioWeb.id_usuario_web,
        id_usuario:     usuario.id_usuario,
        nombre:         usuario.nombre_completo,
        correo,
        foto,
        estado:         usuario.estado,
      });
    } catch (err) {
      return done(err);
    }
  }
));

passport.serializeUser((user, done)   => done(null, user));
passport.deserializeUser((user, done) => done(null, user));

app.use(passport.initialize());
app.use(passport.session());

// ── Usuario disponible en todas las vistas ──────────────
app.use((req, res, next) => {
  res.locals.usuario = req.user || null;
  next();
});

// ── Middleware: solo socios autenticados ─────────────────
function authSocio(req, res, next) {
  if (req.isAuthenticated()) return next();
  res.redirect('/login');
}

// ══════════════════════════════════════════════════════════
//  RUTAS AUTH
// ══════════════════════════════════════════════════════════

app.get('/auth/google',
  passport.authenticate('google', { scope: ['profile', 'email'] })
);

app.get('/auth/google/callback',
  passport.authenticate('google', {
    failureRedirect: '/login?error=no_registrado',
    successRedirect: '/dashboard',
  })
);

app.get('/auth/logout', (req, res, next) => {
  req.logout((err) => {
    if (err) return next(err);
    res.redirect('/');
  });
});

// ══════════════════════════════════════════════════════════
//  RUTAS PÚBLICAS
// ══════════════════════════════════════════════════════════

app.get('/', async (req, res) => {
  try {
    const [membresias] = await db.query(
      'SELECT * FROM membresias ORDER BY precio ASC'
    );
    res.render('paginas/landing', { membresias });
  } catch (err) {
    console.error('Landing error:', err);
    res.render('paginas/landing', { membresias: [] });
  }
});

app.get('/login', (req, res) => {
  if (req.isAuthenticated()) return res.redirect('/dashboard');
  res.render('paginas/login', { error: req.query.error || null });
});

app.get('/registro', (req, res) => {
  res.render('paginas/registro');
});

app.get('/info', async (req, res) => {
  let seccion = req.query.s || 'general';
  if (seccion === 'instalaciones') seccion = 'general'; // merged into general
  let membresias = [];
  if (seccion === 'planes') {
    try {
      const [rows] = await db.query(
        `SELECT id_membresia, nombre_membresia, precio, duracion_dias, descripcion, estado
         FROM membresias
         WHERE estado IN ('activa','oferta','temporada')
         ORDER BY precio ASC`
      );
      membresias = rows;
    } catch (err) {
      console.error('Info/planes error:', err);
    }
  }

  let clases = [];
  if (seccion === 'horarios') {
    try {
      const [rows] = await db.query('SELECT * FROM clases ORDER BY costo ASC');
      clases = rows;
    } catch (err) {
      console.error('Info/horarios error:', err);
    }
  }

  res.render('paginas/info', { seccion, membresias, clases });
});

app.get('/tienda', (req, res) => {
  res.render('paginas/tienda', { nav: 'tienda' });
});

app.get('/hazte-socio', async (req, res) => {
  try {
    const [membresias] = await db.query(
      `SELECT id_membresia, nombre_membresia, precio, duracion_dias, descripcion, estado
       FROM membresias
       WHERE estado IN ('activa','oferta','temporada')
       ORDER BY precio ASC`
    );
    res.render('paginas/hazte-socio', { nav: 'hazte-socio', membresias });
  } catch (err) {
    console.error('Hazte-socio error:', err);
    res.render('paginas/hazte-socio', { nav: 'hazte-socio', membresias: [] });
  }
});

app.post('/api/pre-registro', async (req, res) => {
  try {
    const { nombre_completo, edad, ciudad, correo, telefono, telefono_emergencia, id_membresia } = req.body;
    if (!nombre_completo || !nombre_completo.trim()) {
      return res.status(400).json({ ok: false, error: 'El nombre es obligatorio.' });
    }
    const expira_en = new Date(Date.now() + 60 * 1000);
    const [result] = await db.query(
      `INSERT INTO pre_registros
         (nombre_completo, edad, ciudad, correo, telefono, telefono_emergencia, id_membresia, expira_en)
       VALUES (?, ?, ?, ?, ?, ?, ?, ?)`,
      [
        nombre_completo.trim(),
        edad || null,
        (ciudad || 'Los Mochis').trim(),
        correo || null,
        telefono || null,
        telefono_emergencia || null,
        id_membresia || null,
        expira_en,
      ]
    );
    res.json({ ok: true, id: result.insertId, expira_en });
  } catch (err) {
    console.error('Pre-registro error:', err);
    res.status(500).json({ ok: false, error: 'Error interno del servidor.' });
  }
});

app.get('/competencias', (req, res) => {
  res.render('paginas/competencias', { nav: 'competencias' });
});

// ══════════════════════════════════════════════════════════
//  RUTAS SOCIO — requieren sesión activa
// ══════════════════════════════════════════════════════════

app.get('/dashboard', authSocio, async (req, res) => {
  try {
    const correo = req.user.correo;

    // Usuario + membresía + tag + fechas calculadas
    const [[perfil]] = await db.query(`
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
      FROM usuarios u
      JOIN  membresias m ON u.id_membresia = m.id_membresia
      LEFT JOIN tags   t ON u.id_tag       = t.id_tag
      WHERE u.correo = ?
      LIMIT 1
    `, [correo]);

    if (!perfil) return res.redirect('/login');

    // Asistencias, clase y records en paralelo
    const [asistRes, claseRes, recordsRes] = await Promise.all([
      db.query('SELECT COUNT(*) AS total FROM acceso WHERE id_usuario = ?', [perfil.id_usuario]),
      perfil.id_clase
        ? db.query('SELECT * FROM clases WHERE id_clase = ? LIMIT 1', [perfil.id_clase])
        : Promise.resolve([[null]]),
      db.query('SELECT * FROM records_personales WHERE id_usuario = ? ORDER BY id_pr', [perfil.id_usuario]),
    ]);

    const total_asistencias = asistRes[0][0]?.total  ?? 0;
    const clase             = claseRes[0][0]         || null;
    const records           = recordsRes[0]          || [];

    const fmt = d => d
      ? new Date(d).toLocaleDateString('es-MX', { day: 'numeric', month: 'short', year: 'numeric' })
      : '—';

    res.render('paginas/dashboard', {
      nav: 'dashboard',
      perfil: {
        ...perfil,
        fecha_inscripcion_fmt: fmt(perfil.fecha_inscripcion),
        fecha_renovacion_fmt:  fmt(perfil.fecha_renovacion),
        fecha_vencimiento_fmt: fmt(perfil.fecha_vencimiento),
      },
      total_asistencias,
      clase,
      records,
    });
  } catch (err) {
    console.error('Dashboard error:', err);
    res.render('paginas/dashboard', { nav: 'dashboard', perfil: null, total_asistencias: 0, clase: null, records: [] });
  }
});

app.get('/mi-progreso', authSocio, (req, res) => {
  res.render('paginas/mi-progreso', { nav: 'mi-progreso' });
});

app.get('/clases', authSocio, (req, res) => {
  res.render('paginas/clases', { nav: 'clases' });
});

app.get('/pagos', authSocio, (req, res) => {
  res.render('paginas/pagos', { nav: 'pagos' });
});

app.get('/acceso-qr', authSocio, (req, res) => {
  res.render('paginas/acceso-qr', { nav: 'acceso-qr' });
});

// ── Limpieza de pre-registros expirados ──────────────────
setInterval(async () => {
  try {
    await db.query(
      `UPDATE pre_registros SET estado = 'expirado'
       WHERE estado = 'pendiente' AND expira_en < NOW()`
    );
  } catch (err) {
    console.error('Cleanup pre_registros error:', err);
  }
}, 60_000);

// ── Iniciar ──────────────────────────────────────────────
app.listen(PORT, () => {
  console.log(`Byle Web v2 → http://localhost:${PORT}`);
});
