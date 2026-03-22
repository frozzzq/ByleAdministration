const express = require('express');
const path = require('path');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3000;

// ── Motor de vistas EJS ──
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));

// ── Archivos estáticos (CSS, JS, imágenes) ──
app.use(express.static(path.join(__dirname, 'public')));

// ── Middleware ──
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// ══════════════════════════════════════
//  RUTAS — Páginas públicas
// ══════════════════════════════════════

app.get('/', (req, res) => {
  res.render('landing');
});

app.get('/login', (req, res) => {
  res.render('login');
});

// ══════════════════════════════════════
//  RUTAS — Portal del cliente (6 tabs)
// ══════════════════════════════════════

app.get('/dashboard', (req, res) => {
  res.render('dashboard', { activePage: 'dashboard' });
});

app.get('/rutina', (req, res) => {
  res.render('rutina', { activePage: 'rutina' });
});

app.get('/progreso', (req, res) => {
  res.render('progreso', { activePage: 'progreso' });
});

app.get('/clases', (req, res) => {
  res.render('clases', { activePage: 'clases' });
});

app.get('/pagos', (req, res) => {
  res.render('pagos', { activePage: 'pagos' });
});

app.get('/acceso-qr', (req, res) => {
  res.render('acceso-qr', { activePage: 'acceso-qr' });
});

// ══════════════════════════════════════
//  RUTAS API (futuro — Josué)
// ══════════════════════════════════════
// app.use('/api/auth', require('./routes/authRoutes'));
// app.use('/api/clientes', require('./routes/clienteRoutes'));
// app.use('/api/clases', require('./routes/claseRoutes'));
// app.use('/api/pagos', require('./routes/pagoRoutes'));

// ── Iniciar servidor ──
app.listen(PORT, () => {
  console.log(`Byle Web corriendo en http://localhost:${PORT}`);
});
