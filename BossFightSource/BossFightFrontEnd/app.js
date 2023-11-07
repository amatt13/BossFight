const express = require('express');
const app = express();

const fs = require('fs');
const https = require('https');
const WebSocket = require('ws');

/*
const server = new https.createServer({
  cert: fs.readFileSync('/etc/letsencrypt/live/bossfight.ix.tc/fullchain.pem'),
  key: fs.readFileSync('/etc/letsencrypt/live/bossfight.ix.tc/privkey.pem')
});
const wss = new WebSocket.Server({ server });
var msg;

wss.on('connection', function connection(ws)
{
  ws.on('message', function incoming(message)
  {
    msg = message;
    console.log('received: %s', msg);
    wss.clients.forEach(function (client)
    {
       if (client.readyState == WebSocket.OPEN)
       {
          client.send( msg );
       }
    });
  });
});

console.log("Boss Fight Socket Server is now listening on port 5000...")
server.listen(5000);
*/


let options = {};
let https_enabled = false;
let port = 80;
var site_server = null;

try {
        options = {
                key: fs.readFileSync("/etc/letsencrypt/live/bossfight.ix.tc/privkey.pem"),
                cert: fs.readFileSync("/etc/letsencrypt/live/bossfight.ix.tc/fullchain.pem"),
                ca: fs.readFileSync("/etc/letsencrypt/live/bossfight.ix.tc/chain.pem"),
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
