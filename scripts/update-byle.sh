#!/bin/bash

echo ">>> Haciendo pull..."
cd /home/frozz/ByleAdministration_repositorio/ByleAdministration
git pull

echo ">>> Reiniciando app..."
runuser -l frozz -c "cd ~/ByleAdministration_repositorio/ByleAdministration/web/byle-web && pm2 restart byle-web 2>/dev/null || (pm2 start src/server.js --name byle-web && pm2 save)"

echo ">>> Iniciando cloudflared..."
runuser -l frozz -c "cloudflared tunnel run byle-tunnel"

echo ">>> Listo!"