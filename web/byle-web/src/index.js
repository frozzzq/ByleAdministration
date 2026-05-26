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

app.get('/', (req, res) => {
  res.render('paginas/landing');
});

app.get('/login', (req, res) => {
  if (req.isAuthenticated()) return res.redirect('/dashboard');
  res.render('paginas/login', { error: req.query.error || null });
});

app.get('/registro', (req, res) => {
  res.render('paginas/registro');
});

app.get('/info', (req, res) => {
  res.render('paginas/info', { seccion: req.query.s || 'general' });
});

app.get('/tienda', (req, res) => {
  res.render('paginas/tienda', { nav: 'tienda' });
});

app.get('/hazte-socio', (req, res) => {
  res.render('paginas/hazte-socio', { nav: 'hazte-socio' });
});

app.get('/competencias', (req, res) => {
  res.render('paginas/competencias', { nav: 'competencias' });
});

// ══════════════════════════════════════════════════════════
//  RUTAS SOCIO — requieren sesión activa
// ══════════════════════════════════════════════════════════

app.get('/dashboard', authSocio, (req, res) => {
  res.render('paginas/dashboard', { nav: 'dashboard' });
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

// ── Iniciar ──────────────────────────────────────────────
app.listen(PORT, () => {
  console.log(`Byle Web v2 → http://localhost:${PORT}`);
});
