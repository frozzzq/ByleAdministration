const express        = require('express');
const router         = express.Router();
const AuthController = require('../controllers/AuthController');

router.get('/auth/google',          AuthController.googleLogin);
router.get('/auth/google/callback', AuthController.googleCallback);
router.get('/auth/logout',          AuthController.logout);

module.exports = router;
