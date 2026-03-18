const express = require('express');
const app = express();

app.get('/', (req, res) => {
	res.send('<h1>KAZZURA JOTO JAJAJAJAAJAJ LOL</h1/>');
});

app.listen(3000, () => {
	console.log('servidor corriendo en puerto 3000');
});
