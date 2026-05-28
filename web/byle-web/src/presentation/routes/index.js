const express = require('express');
const router  = express.Router();

router.use(require('./auth.routes'));
router.use(require('./public.routes'));
router.use(require('./private.routes'));

module.exports = router;
