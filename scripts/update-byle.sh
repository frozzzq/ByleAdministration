#!/bin/bash

echo ">>> Haciendo pull..."
cd /home/frozz/ByleAdministration_repositorio/ByleAdministration
git pull

echo ">>> Reiniciando app..."
runuser -l frozz -c "pm2 restart byle-web"

echo ">>> Iniciando cloudflared..."
runuser -l frozz -c "cloudflared tunnel run byle-tunnel"

echo ">>> Listo!"