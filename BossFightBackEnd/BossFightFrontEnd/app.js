const express = require('express')
const app = express()
var server = require('http').createServer(app);
const port = 3000

app.use("/", express.static(__dirname));
app.get('/', function (req, res) {
	res.sendFile(__dirname + '/index.html');
});

server.listen(port, () => {
	console.log(`Example app listening at http://localhost:${port}`)
})
