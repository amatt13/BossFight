function init() {
	window.requestAnimationFrame(ts_draw);
}

function ts_draw() {
	Draw()
	window.requestAnimationFrame(ts_draw);
}

init();


function ReSizeCanvas() {
	if (canvas.width != Math.floor(window.innerWidth * 0.25) || canvas.height != Math.floor(window.innerHeight * 0.90)) {
		canvas.width = Math.floor(window.innerWidth * 0.25);
		canvas.height = Math.floor(window.innerHeight * 0.90);
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
	ctx.fillStyle = "hsl(349, 19%, 45%)"
	ctx.fillText('monsterImageY:' + monsterImageY.toString(), 300, 50);
	ctx.fillText('initialMMonsterImageY:' + initialMMonsterImageY.toString(), 300, 100);
	ctx.fillText('monsterImageMoveDown: ' + monsterImageMoveDown.toString(), 300, 150);
}