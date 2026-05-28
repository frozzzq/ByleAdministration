const GetDashboardUseCase = require('../../application/usecases/GetDashboardUseCase');

module.exports = {
  async dashboard(req, res) {
    try {
      const data = await GetDashboardUseCase.execute(req.user.correo);
      if (!data) return res.redirect('/login');
      res.render('paginas/dashboard', { nav: 'dashboard', ...data });
    } catch (err) {
      console.error('Dashboard error:', err);
      res.render('paginas/dashboard', {
        nav:               'dashboard',
        perfil:            null,
        total_asistencias: 0,
        clase:             null,
        records:           [],
      });
    }
  },

  miProgreso(req, res) {
    res.render('paginas/mi-progreso', { nav: 'mi-progreso' });
  },

  clases(req, res) {
    res.render('paginas/clases', { nav: 'clases' });
  },

  pagos(req, res) {
    res.render('paginas/pagos', { nav: 'pagos' });
  },

  accesoQr(req, res) {
    res.render('paginas/acceso-qr', { nav: 'acceso-qr' });
  },
};
