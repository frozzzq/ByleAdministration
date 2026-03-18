const express = require('express');
const app = express();

app.get('/', (req, res) => {
  res.send(`
    <!DOCTYPE html>
    <html lang="es">
    <head>
      <meta charset="UTF-8">
      <title>Byle</title>
      <style>
        body {
          margin: 0;
          font-family: Arial, sans-serif;
          background: #0f0f0f;
          color: white;
          display: flex;
          justify-content: center;
          align-items: center;
          height: 100vh;
        }
        h1 { font-size: 3rem; }
        p  { color: #aaa; }
      </style>
    </head>
    <body>
      <div style="text-align:center">
        <h1>⚡ Byle</h1>
        <p>Servidor funcionando correctamente</p>
      </div>
    </body>
    </html>
  `);
});

app.listen(3000, () => {
  console.log('servidor corriendo en puerto 3000');
});