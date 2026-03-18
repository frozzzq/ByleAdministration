echo "haciendo pull..."
cd ~/ByleAdministration_repositorio/ByleAdministration
git pull

echo "reiniciando app..."
pm2 restart byle-web

echo "listo!"
