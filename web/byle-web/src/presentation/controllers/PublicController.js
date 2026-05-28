const MembresiaRepository      = require('../../infrastructure/repositories/MembresiaRepository');
const GetInfoUseCase           = require('../../application/usecases/GetInfoUseCase');
const CreatePreRegistroUseCase = require('../../application/usecases/CreatePreRegistroUseCase');
const AppError                 = require('../../domain/errors/AppError');

module.exports = {
  async landing(req, res) {
    try {
      const membresias = await MembresiaRepository.getAll();
      res.render('paginas/landing', { membresias });
    } catch (err) {
      console.error('Landing error:', err);
      res.render('paginas/landing', { membresias: [] });
    }
  },

  login(req, res) {
    if (req.isAuthenticated()) return res.redirect('/dashboard');
    res.render('paginas/login', { error: req.query.error || null });
  },

  registro(req, res) {
    res.render('paginas/registro');
  },

  async info(req, res) {
    try {
      const data = await GetInfoUseCase.execute(req.query.s);
      res.render('paginas/info', data);
    } catch (err) {
      console.error('Info error:', err);
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
      console.error('Hazte-socio error:', err);
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
      console.error('Pre-registro error:', err);
      res.status(500).json({ ok: false, error: 'Error interno del servidor.' });
    }
  },

  competencias(req, res) {
    res.render('paginas/competencias', { nav: 'competencias' });
  },
};
