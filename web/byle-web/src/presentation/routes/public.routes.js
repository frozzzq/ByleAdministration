const express              = require('express');
const router               = express.Router();
const PublicController     = require('../controllers/PublicController');
const { preRegistroLimiter } = require('../middleware/rate-limit.middleware');

router.get('/',                                       PublicController.landing);
router.get('/login',                                  PublicController.login);
router.get('/registro',                               PublicController.registro);
router.get('/info',                                   PublicController.info);
router.get('/tienda',                                 PublicController.tienda);
router.get('/hazte-socio',                            PublicController.hazteSocio);
router.post('/api/pre-registro', preRegistroLimiter,  PublicController.crearPreRegistro);
router.get('/competencias',                           PublicController.competencias);

module.exports = router;
