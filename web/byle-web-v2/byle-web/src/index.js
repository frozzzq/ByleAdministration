const express = require('express');
const path = require('path');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3000;

// ── Motor de vistas ──
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));

// ── Archivos estáticos ──
app.use(express.static(path.join(__dirname, 'public')));

// ── Middleware ──
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// ══════════════════════════════════════════════════
//  RUTAS PÚBLICAS — cualquiera puede ver
// ══════════════════════════════════════════════════

app.get('/', (req, res) => {
  res.render('paginas/landing');
});

app.get('/login', (req, res) => {
  res.render('paginas/login');
});

app.get('/registro', (req, res) => {
  res.render('paginas/registro');
});

app.get('/info', (req, res) => {
  res.render('paginas/info', { seccion: req.query.s || 'general' });
});

// ══════════════════════════════════════════════════
//  RUTAS GOOGLE / VISITANTE — cualquier usuario logueado
// ══════════════════════════════════════════════════

app.get('/tienda', (req, res) => {
  // TODO: Futuro → SELECT * FROM productos WHERE stock > 0
  res.render('paginas/tienda', { nav: 'tienda' });
});

app.get('/hazte-socio', (req, res) => {
  res.render('paginas/hazte-socio', { nav: 'hazte-socio' });
});

// ══════════════════════════════════════════════════
//  RUTAS COMPETENCIAS — visibles para todos,
//  inscripción solo socios
// ══════════════════════════════════════════════════

app.get('/competencias', (req, res) => {
  // TODO: Futuro → SELECT con JOINs a acceso + usuarios
  res.render('paginas/competencias', { nav: 'competencias' });
});

// ══════════════════════════════════════════════════
//  RUTAS SOCIO — solo usuarios con membresía activa
//  TODO: Middleware authSocio que verifique sesión + estado
// ══════════════════════════════════════════════════

app.get('/dashboard', (req, res) => {
  // TODO: SELECT u.*, m.nombre_membresia, m.precio
  //       FROM usuarios u
  //       JOIN membresias m ON u.id_membresia = m.id_membresia
  //       WHERE u.id_usuario = ?
  res.render('paginas/dashboard', { nav: 'dashboard' });
});

app.get('/mi-progreso', (req, res) => {
  // TODO: SELECT * FROM records_personales WHERE id_usuario = ?
  res.render('paginas/mi-progreso', { nav: 'mi-progreso' });
});

app.get('/clases', (req, res) => {
  // TODO: SELECT * FROM clases WHERE cupos > 0
  res.render('paginas/clases', { nav: 'clases' });
});

app.get('/pagos', (req, res) => {
  // TODO: SELECT * FROM ventas WHERE id_usuario = ? ORDER BY fecha DESC
  res.render('paginas/pagos', { nav: 'pagos' });
});

app.get('/acceso-qr', (req, res) => {
  res.render('paginas/acceso-qr', { nav: 'acceso-qr' });
});

// ══════════════════════════════════════════════════
//  RUTAS API — futuro (Josué)
// ══════════════════════════════════════════════════
// app.post('/api/auth/login', ...)
// app.post('/api/auth/google', ...)
// app.get('/api/productos', ...)          → SELECT * FROM productos WHERE stock > 0
// app.post('/api/ordenes', ...)           → INSERT INTO orden_web (...)
// app.get('/api/competencias/:cat', ...)  → ranking por categoría
// app.post('/api/pre-registro', ...)      → INSERT temporal con TTL 1 hora
// app.get('/api/cliente/perfil', ...)
// app.get('/api/cliente/pagos', ...)
// app.get('/api/cliente/clases', ...)

// ── Iniciar ──
app.listen(PORT, () => {
  console.log(`Byle Web v2 → http://localhost:${PORT}`);
});
