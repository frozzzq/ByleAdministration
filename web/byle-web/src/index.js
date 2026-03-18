const express = require('express');
const app = express();

app.get('/', (req, res) => {
  res.send(`
    <!DOCTYPE html>
    <html lang="es">
    <head>
      <meta charset="UTF-8">
      <title>Byle</title>
      <link href="https://fonts.googleapis.com/css2?family=Bebas+Neue&family=Inter:wght@300;400&display=swap" rel="stylesheet">
      <style>
        *, *::before, *::after { margin: 0; padding: 0; box-sizing: border-box; }

        :root {
          --bg: #080808;
          --accent: #c8f135;
          --text: #f0f0f0;
          --muted: #444;
        }

        body {
          background: var(--bg);
          color: var(--text);
          font-family: 'Inter', sans-serif;
          height: 100vh;
          display: flex;
          flex-direction: column;
          justify-content: center;
          align-items: center;
          overflow: hidden;
        }

        .noise {
          position: fixed;
          inset: 0;
          background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noise'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noise)' opacity='0.04'/%3E%3C/svg%3E");
          pointer-events: none;
          z-index: 10;
        }

        .line {
          width: 1px;
          height: 80px;
          background: var(--accent);
          margin-bottom: 2rem;
          animation: grow 1s ease forwards;
          transform-origin: top;
        }

        @keyframes grow {
          from { transform: scaleY(0); opacity: 0; }
          to   { transform: scaleY(1); opacity: 1; }
        }

        h1 {
          font-family: 'Bebas Neue', sans-serif;
          font-size: clamp(5rem, 18vw, 12rem);
          letter-spacing: 0.05em;
          line-height: 1;
          color: var(--text);
          animation: fadeUp 0.8s 0.3s ease both;
        }

        h1 span { color: var(--accent); }

        .sub {
          font-size: 0.75rem;
          font-weight: 300;
          letter-spacing: 0.35em;
          text-transform: uppercase;
          color: var(--muted);
          margin-top: 1.2rem;
          animation: fadeUp 0.8s 0.5s ease both;
        }

        .badge {
          margin-top: 2.5rem;
          padding: 0.4rem 1rem;
          border: 1px solid var(--accent);
          color: var(--accent);
          font-size: 0.65rem;
          letter-spacing: 0.2em;
          text-transform: uppercase;
          animation: fadeUp 0.8s 0.7s ease both;
        }

        @keyframes fadeUp {
          from { opacity: 0; transform: translateY(20px); }
          to   { opacity: 1; transform: translateY(0); }
        }
      </style>
    </head>
    <body>
      <div class="noise"></div>
      <div class="line"></div>
      <h1>BY<span>LE</span></h1>
      <p class="sub">Sistema de gestión para gimnasios</p>
      <div class="badge">⚡ Servidor activo</div>
    </body>
    </html>
  `);
});

app.listen(3000, () => {
  console.log('servidor corriendo en puerto 3000');
});