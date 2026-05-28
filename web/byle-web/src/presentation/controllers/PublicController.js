const MembresiaRepository      = require('../../infrastructure/repositories/MembresiaRepository');
const GetInfoUseCase           = require('../../application/usecases/GetInfoUseCase');
const CreatePreRegistroUseCase = require('../../application/usecases/CreatePreRegistroUseCase');
const AppError                 = require('../../domain/errors/AppError');

// Valores permitidos para el parámetro ?error en /login
const ERRORES_VALIDOS = new Set(['no_registrado']);

module.exports = {
  async landing(req, res) {
    try {
      const membresias = await MembresiaRepository.getAll();
      res.render('paginas/landing', { membresias });
    } catch (err) {
      console.error('Landing error:', err.message);
      res.render('paginas/landing', { membresias: [] });
    }
  },

  login(req, res) {
    if (req.isAuthenticated()) return res.redirect('/dashboard');
    // Validar contra whitelist — evita que valores arbitrarios lleguen a la vista
    const error = ERRORES_VALIDOS.has(req.query.error) ? req.query.error : null;
    res.render('paginas/login', { error });
  },

  registro(req, res) {
    res.render('paginas/registro');
  },

  async info(req, res) {
    try {
      const data = await GetInfoUseCase.execute(req.query.s);
      res.render('paginas/info', data);
    } catch (err) {
      console.error('Info error:', err.message);
      res.render('paginas/info', { seccion: 'general', membresias: [], clases: [] });
    }
  },

  tienda(req, res) {
    res.render('paginas/tienda', { nav: 'tienda' });
  },

  async hazteSocio(req, res) {
    try {
      const membresias = await MembresiaRepository.getActivas();
      res.render('paginas/hazte-socio', { nav: 'hazte-socio', membresias });
    } catch (err) {
      console.error('Hazte-socio error:', err.message);
      res.render('paginas/hazte-socio', { nav: 'hazte-socio', membresias: [] });
    }
  },

  async crearPreRegistro(req, res) {
    try {
      const result = await CreatePreRegistroUseCase.execute(req.body);
      res.json({ ok: true, ...result });
    } catch (err) {
      if (err instanceof AppError && err.statusCode < 500) {
        return res.status(err.statusCode).json({ ok: false, error: err.message });
      }
      console.error('Pre-registro error:', err.message);
      res.status(500).json({ ok: false, error: 'Error interno del servidor.' });
    }
  },

  competencias(req, res) {
    res.render('paginas/competencias', { nav: 'competencias' });
  },
};
