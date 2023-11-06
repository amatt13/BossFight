const fs = require('fs');
const express = require('express')
const app = express()

let options = {};
let https = false;
let port = 80;
var server = null;

try {
	options = {
		key: fs.readFileSync("/etc/letsencrypt/live/bossfight.ix.tc/privkey.pem"),
		cert: fs.readFileSync("/etc/letsencrypt/live/bossfight.ix.tc/fullchain.pem"),
		ca: fs.readFileSync("/etc/letsencrypt/live/bossfight.ix.tc/chain.pem"),
	};
	https = true;
	port = 443;
	server = require('https').createServer(options, app);
}
catch (error) {
	console.warn("Could not load ceertificates");
	server = require('http').createServer(options, app);
  }

app.use("/", express.static(__dirname));
app.use(express.static(__dirname + '/static', { dotfiles: 'allow' }));
app.get('/', function (req, res) {
        res.sendFile(__dirname + '/Index.html');
});

// check if a fille in "/images/sprites/player_classes/" exists
app.get('/findfiles/*', function (req, res) {
        let path = '.' + req["url"].replace("findfiles", "/images/sprites/player_classes/");
        path = path.replace("%20", " ").toLowerCase();
        console.log(path);
        if (fs.existsSync(path)) {
                res.status(200).send({"filefound": true});
          } else {
                res.status(404).send({"filefound": false});
                console.log("NOT found")
          }
});

app.get('/render/', function (req, res) {
        res.sendFile(__dirname + 'render/index.html');
});

server.listen(port, function (){
	if (https) {
		console.log(`Boss Fight FE listening at https://localhost:${port}`)
	}
	else {
		console.log(`Boss Fight FE listening at http://localhost:${port}`)
	}
})
