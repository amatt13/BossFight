const canvas = document.getElementById('mainCanvas');
const ctx = canvas.getContext('2d');
var monsterImage = new Image();
monsterImage.src = "images/sprites/monsters/goblin.png";
let initialMonsterImageX = 0, initialMMonsterImageY = -1;

monsterImage.addEventListener("load", () => {
	initialMMonsterImageY += monsterImage.height;
	ctx.drawImage(monsterImage, initialMonsterImageX, initialMMonsterImageY);
});

// TEST GET
async function SendTestGETRequest() {
	const response = await fetch("https://localhost:5001/WeatherForecast", {
		method: 'GET',
		headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
	});

	if (!response.ok) 
	{

	}

	if (response.body !== null) {
		const body = await response.json();
		console.log(body);
	}
}

// TEST POST
async function SendTestPOSTRequest() {
	const obj = { id: 101, testString: 'fucking ', unusedString: 'yes' };
	const response = await fetch(`https://localhost:5001/PostTest`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json; charset=UTF-8' },
		body: JSON.stringify(obj)
	});

	if (!response.ok) {
		
	}

	if (response.body !== null) {
		const body = await response.json();
		console.log(body);
	}
}

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
