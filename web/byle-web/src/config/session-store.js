const session    = require('express-session');
const MySQLStore = require('express-mysql-session')(session);

const sessionStore = new MySQLStore({
  host:                    process.env.DB_HOST,
  user:                    process.env.DB_USER,
  password:                process.env.DB_PASSWORD,
  database:                process.env.DB_NAME,
  createDatabaseTable:     true,
  clearExpired:            true,
  checkExpirationInterval: 15 * 60 * 1000, // limpiar expiradas cada 15 min
  expiration:              7 * 24 * 60 * 60 * 1000,
  connectionLimit:         3,
  charset:                 'utf8mb4_unicode_ci',
});

module.exports = sessionStore;
