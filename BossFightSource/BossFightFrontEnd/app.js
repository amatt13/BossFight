const fs = require('fs');
const express = require('express')
const app = express()
var server = require('http').createServer(app);
const port = 3000

app.use("/", express.static(__dirname));
app.get('/', function (req, res) {
	res.sendFile(__dirname + '/Index.html');
});

// check if a fille in "/images/sprites/player_classes/" exists
app.get('/findfiles/*', function (req, res) {
	let path = '.' + req["url"].replace("findfiles", "/images/sprites/player_classes/");
	path = path.replace("%20", " ");
	console.log(path);
	if (fs.existsSync(path)) {
		res.status(200).send({"filefound": true});
		console.log("found")
	  } else {
		res.status(404).send({"filefound": false});
		console.log("NOT found")
	  }
});

app.get('/render/', function (req, res) {
	res.sendFile(__dirname + 'render/index.html');
});

server.listen(port, () => {
	console.log(`Boss Fight FE listening at http://localhost:${port}`)
})
