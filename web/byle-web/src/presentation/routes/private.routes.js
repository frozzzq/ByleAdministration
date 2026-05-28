const express           = require('express');
const router            = express.Router();
const { authSocio }     = require('../middleware/auth.middleware');
const PrivateController = require('../controllers/PrivateController');

router.get('/dashboard',   authSocio, PrivateController.dashboard);
router.get('/mi-progreso', authSocio, PrivateController.miProgreso);
router.get('/clases',      authSocio, PrivateController.clases);
router.get('/pagos',       authSocio, PrivateController.pagos);
router.get('/acceso-qr',   authSocio, PrivateController.accesoQr);

module.exports = router;
