#!/bin/bash

echo ">>> Iniciando app por primera vez..."
cd ~/ByleAdministration_repositorio/ByleAdministration/web/byle-web
pm2 start src/server.js --name byle-web
pm2 save

echo ">>> Iniciando cloudflared..."
cloudflared tunnel run byle-tunnel

echo ">>> Listo!"
