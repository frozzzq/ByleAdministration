const express  = require('express');
const path     = require('path');
const session  = require('express-session');
const passport = require('./config/passport');
const routes   = require('./presentation/routes');

const app = express();

// ── Vistas ───────────────────────────────────────────────
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));
app.use(express.static(path.join(__dirname, 'public')));

// ── Parsers ──────────────────────────────────────────────
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// ── Sesiones ────────────────────────────────────────────
app.use(session({
  secret:            process.env.SESSION_SECRET || 'byle-secret',
  resave:            false,
  saveUninitialized: false,
  cookie: { httpOnly: true, secure: false, maxAge: 7 * 24 * 60 * 60 * 1000 },
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
