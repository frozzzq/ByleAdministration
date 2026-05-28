const express        = require('express');
const router         = express.Router();
const AuthController = require('../controllers/AuthController');
const { authLimiter } = require('../middleware/rate-limit.middleware');

router.get('/auth/google',          authLimiter, AuthController.googleLogin);
router.get('/auth/google/callback', AuthController.googleCallback);

// POST para logout — previene CSRF logout via GET (img tag / enlace externo)
router.post('/auth/logout', AuthController.logout);

module.exports = router;
