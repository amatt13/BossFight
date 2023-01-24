const express = require('express')
const app = express()
var server = require('http').createServer(app);
const port = 3000

app.use("/", express.static(__dirname));
app.get('/', function (req, res) {
	res.sendFile(__dirname + '/Index.html');
});

server.listen(port, () => {
	console.log(`Boss Fight FE listening at http://localhost:${port}`)
})
