const canvas = document.getElementById('mainCanvas');
const ctx = canvas.getContext('2d');
var monsterImage = new Image();
monsterImage.src = "images/sprites/monsters/goblin.png";
let initialMonsterImageX = 0, initialMMonsterImageY = -1;

monsterImage.addEventListener("load", () => {
	initialMMonsterImageY += monsterImage.height;
	ctx.drawImage(monsterImage, initialMonsterImageX, initialMMonsterImageY);
});


let socket = new WebSocket("ws://localhost:5000/ws");

socket.onopen = function(e) {
	LogToTextLog("[open] Connection established")
};

socket.onmessage = function(event) {
	LogToTextLog(`[message] Data received from server: ${event.data}`);
	var json_dict = JSON.parse(event.data);
	_player = new Player(json_dict["Name"], json_dict["Level"], json_dict["HP"], json_dict["Mana"], json_dict["Gold"]);
	UpdateUiPlayerStats(_player);
};

socket.onclose = function(event) {
	if (event.wasClean) {
		LogToTextLog(`[close] Connection closed cleanly, code=${event.code} reason=${event.reason}`);
	} else {
		// e.g. server process killed or network down
		// event.code is usually 1006 in this case
		LogToTextLog('[close] Connection died');
	}
};

socket.onerror = function(error) {
	LogToTextLog(`[error] ${error.message}`);
};

// TEST Get WebSocket button
async function LoginTestUser() {
	const obj = { 
		request_key: "FetchPlayer", 
		request_data: JSON.stringify({
			player_id: "1337"
		})
	};
	const json_obj = JSON.stringify(obj);
	LogToTextLog("Sending to server");
	socket.send(json_obj);
}



function LogToTextLog(pText)
{
	document.getElementById("text_log").innerHTML += pText + '\n'
}


function UpdateUiPlayerStats(player) {
	document.getElementById("player_name").innerHTML = player.name;
	document.getElementById("player_Level").innerHTML = player.level; 
	document.getElementById("player_hp").innerHTML = player.hp;
	document.getElementById("player_mana").innerHTML = player.mana;
	document.getElementById("player_gold").innerHTML = player.gold;
}


class Player {
	constructor(name, level, hp, mana, gold) {
		this.name = name;
		this.level = level;
		this.hp = hp;
		this.mana = mana;
		this.gold = gold;
	}
}

let _player = new Player();