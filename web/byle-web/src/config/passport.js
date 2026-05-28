const passport       = require('passport');
const { Strategy: GoogleStrategy } = require('passport-google-oauth20');
const UsuarioRepository = require('../infrastructure/repositories/UsuarioRepository');

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

      const usuario = await UsuarioRepository.findByCorreo(correo);
      if (!usuario) return done(null, false, { message: 'no_registrado' });

      let usuarioWeb = await UsuarioRepository.findWebByCorreo(correo);

      if (!usuarioWeb) {
        const newId = await UsuarioRepository.createUsuarioWeb(correo, usuario.telefono, googleId, foto);
        usuarioWeb = { id_usuario_web: newId };
        if (!usuario.id_usuario_web) {
          await UsuarioRepository.linkUsuarioWeb(usuario.id_usuario, newId);
        }
      } else {
        await UsuarioRepository.updateGoogleCredentials(usuarioWeb.id_usuario_web, googleId, foto);
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

module.exports = passport;
