echo "haciendo pull..."
cd ~/ByleAdministration_repositorio/ByleAdministration
git pull

echo "reiniciando app..."

pm2 restart byle-web
cloudflared tunnel run --url http://localhost:3000 byle-tunnel

echo "listo!"
