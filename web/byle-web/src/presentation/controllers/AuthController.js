const passport = require('passport');

module.exports = {
  googleLogin:    passport.authenticate('google', { scope: ['profile', 'email'] }),
  googleCallback: passport.authenticate('google', {
    failureRedirect: '/login?error=no_registrado',
    successRedirect: '/dashboard',
  }),

  logout(req, res, next) {
    req.logout((err) => {
      if (err) return next(err);
      res.redirect('/');
    });
  },
};
