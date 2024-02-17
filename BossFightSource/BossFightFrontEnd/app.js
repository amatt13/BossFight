const express = require('express');
const app = express();

const fs = require('fs');
const https = require('https');
const WebSocket = require('ws');

let options = {};
let https_enabled = false;
let port = 80;
var site_server = null;

try {
        options = {
                key: fs.readFileSync("/etc/letsencrypt/live/bossfight.uk.to/privkey.pem"),
                cert: fs.readFileSync("/etc/letsencrypt/live/bossfight.uk.to/fullchain.pem"),
                ca: fs.readFileSync("/etc/letsencrypt/live/bossfight.uk.to/chain.pem"),
        };
        https_enabled = true;
        port = 443;
        site_server = require('https').createServer(options, app);
}
catch (error) {
        console.warn("Could not load ceertificates");
        site_server = require('http').createServer(options, app);
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

site_server.listen(port, function (){
        if (https_enabled) {
                console.log(`Boss Fight FE listening at https://localhost:${port}`)
        }
        else {
                console.log(`Boss Fight FE listening at http://localhost:${port}`)
        }
})
