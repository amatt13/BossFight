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
	document.getElementById("player_name").innerHTML = json_dict["Name"];
	document.getElementById("player_Level").innerHTML = json_dict["Level"];
	document.getElementById("player_hp").innerHTML = json_dict["HP"];
	document.getElementById("player_mana").innerHTML = json_dict["Mana"];
	document.getElementById("player_gold").innerHTML = json_dict["Gold"];
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



// canvas stuff
function ReSizeCanvas() {
	if (canvas.width != Math.floor(window.innerWidth * 0.33) || canvas.height != Math.floor(window.innerHeight * 0.66)) {
		canvas.width = Math.floor(window.innerWidth * 0.33);
		canvas.height = Math.floor(window.innerHeight * 0.66);
		initialMMonsterImageY = canvas.height / 2;
		monsterImageX = initialMonsterImageX, monsterImageY = initialMMonsterImageY;
		monsterImageMoveForward = true, monsterImageMoveDown = false;
	}
}

function Draw() {
	ReSizeCanvas();  // MUST be the first thing that happens
	ctx.globalCompositeOperation = 'destination-over';
	ctx.clearRect(0, 0, canvas.width, canvas.height); // clear canvas
	AnimateMonster();
}

let monsterImageX = initialMonsterImageX, monsterImageY = initialMMonsterImageY;
let monsterImageMoveForward = true, monsterImageMoveDown = false;
function AnimateMonster() {
	if (initialMMonsterImageY == -1)
		initialMMonsterImageY = canvas.height / 2;

	if (monsterImageX >= initialMonsterImageX + 100) {
		monsterImageMoveForward = false;
	}
	else if (monsterImageX <= 0) {
		monsterImageMoveForward = true;
	}

	if (monsterImageY <= initialMMonsterImageY - 50) {
		monsterImageMoveDown = true;
	}
	else if (monsterImageY >= initialMMonsterImageY) {
		monsterImageMoveDown = false;
	}

	monsterImageX = monsterImageMoveForward ? monsterImageX + 0.5 : monsterImageX - 0.5;
	monsterImageY = monsterImageMoveDown ? monsterImageY + 0.5 : monsterImageY - 0.5;

	ctx.drawImage(monsterImage, monsterImageX, monsterImageY);
	ctx.fillText('monsterImageY:' + monsterImageY.toString(), 300, 50);
	ctx.fillText('initialMMonsterImageY:' + initialMMonsterImageY.toString(), 300, 100);
	ctx.fillText('monsterImageMoveDown: ' + monsterImageMoveDown.toString(), 300, 150);
}
