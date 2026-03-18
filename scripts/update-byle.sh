echo "haciendo pull..."
cd /home/frozz/ByleAdministration_repositorio/ByleAdministration
git pull

echo "reiniciando app..."

su -c "pm2 restart byle-web" frozz
cloudflared tunnel run --url http://localhost:3000 byle-tunnel

echo "listo!"
