const express      = require('express');
const path         = require('path');
const session      = require('express-session');
const helmet       = require('helmet');
const passport     = require('./config/passport');
const sessionStore = require('./config/session-store');
const routes       = require('./presentation/routes');

const app          = express();
const isProduction = process.env.NODE_ENV === 'production';

// ── Confiar en Cloudflare (primer proxy) ────────────────
// Necesario para que req.ip devuelva la IP real del cliente
// y para que secure:true funcione detrás de HTTPS de Cloudflare
app.set('trust proxy', 1);

// ── Headers de seguridad HTTP (Helmet) ──────────────────
app.use(helmet({
  contentSecurityPolicy: {
    directives: {
      defaultSrc:     ["'self'"],
      scriptSrc:      ["'self'", "'unsafe-inline'", "cdnjs.cloudflare.com", "cdn.jsdelivr.net", "unpkg.com"],
      styleSrc:       ["'self'", "'unsafe-inline'", "https://fonts.googleapis.com"],
      fontSrc:        ["'self'", "https://fonts.gstatic.com"],
      imgSrc:         ["'self'", "data:", "https:", "*.googleusercontent.com"],
      connectSrc:     ["'self'"],
      frameSrc:       ["'none'"],
      frameAncestors: ["'none'"],  // previene clickjacking
      objectSrc:      ["'none'"],
      baseUri:        ["'self'"],
      formAction:     ["'self'"],  // formularios solo a mismo origen
    },
  },
  crossOriginEmbedderPolicy: false, // necesario para el redirect de Google OAuth
}));

// ── Vistas ───────────────────────────────────────────────
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));
app.use(express.static(path.join(__dirname, 'public')));

// ── Parsers (con límite de tamaño anti-DoS) ──────────────
app.use(express.json({ limit: '10kb' }));
app.use(express.urlencoded({ extended: true, limit: '10kb' }));

// ── Sesiones almacenadas en MySQL ────────────────────────
app.use(session({
  store:             sessionStore,
  secret:            process.env.SESSION_SECRET,
  resave:            false,
  saveUninitialized: false,
  name:              'byle.sid',   // oculta que usamos express-session
  cookie: {
    httpOnly: true,
    secure:   isProduction,        // solo HTTPS en producción
    sameSite: isProduction ? 'strict' : 'lax', // previene CSRF
    maxAge:   7 * 24 * 60 * 60 * 1000,
  },
}));

// ── Passport ─────────────────────────────────────────────
app.use(passport.initialize());
app.use(passport.session());

// ── Usuario disponible en todas las vistas ───────────────
app.use((req, res, next) => {
  res.locals.usuario = req.user || null;
  next();
});

// ── Rutas ────────────────────────────────────────────────
app.use('/', routes);

module.exports = app;
