const canvas = document.getElementById('mainCanvas');
const ctx = canvas.getContext('2d');
var playerImage = document.getElementById("playerSprite");
var monsterImage = document.getElementById("monsterSprite");
let initialMonsterImageX = 30, initialMonsterImageY = -1;

monsterImage.addEventListener("load", () => {
	initialMonsterImageY = canvas.height / 2;
	ctx.drawImage(monsterImage, initialMonsterImageX, initialMonsterImageY);
});

playerImage.addEventListener("load", () => {
	ctx.drawImage(playerImage, canvas.width-playerImage.width, initialMonsterImageY);
});

window.requestAnimationFrame(Draw);

function Draw() {
	ReSizeCanvas();  // MUST be the first thing that happens
	ctx.fillStyle = "hsl(349, 19%, 45%)"
	ResetFont();
	ctx.globalCompositeOperation = 'destination-over';
	ctx.clearRect(0, 0, canvas.width, canvas.height); // clear canvas
	AnimateMonster();
	DrawMonsterStatus();
	AnimatePlayer();
	if (_enable_damage_to_show) {
		DrawDamage();
	}

	window.requestAnimationFrame(Draw);
}

function ReSizeCanvas() {
	if (canvas.width != Math.floor(window.innerWidth * 0.33) || canvas.height != Math.floor(window.innerHeight * 0.90)) {
		canvas.width = Math.floor(window.innerWidth * 0.33);
		canvas.height = Math.floor(window.innerHeight * 0.90);
		initialMonsterImageY = canvas.height / 2;
		monsterImageX = initialMonsterImageX, monsterImageY = initialMonsterImageY;
		monsterImageMoveForward = true, monsterImageMoveDown = false;
	}
}

function ResetFont() {
	ctx.font = "30px myFirstFont";
}

let monsterImageX = initialMonsterImageX, monsterImageY = initialMonsterImageY;
let monsterImageMoveForward = true, monsterImageMoveDown = false;
function AnimateMonster() {
	if (initialMonsterImageY == -1)
		initialMonsterImageY = canvas.height / 2;

	if (monsterImageX >= initialMonsterImageX + 150) {
		monsterImageMoveForward = false;
	}
	else if (monsterImageX <= 30) {
		monsterImageMoveForward = true;
	}

	if (monsterImageY <= initialMonsterImageY - 50) {
		monsterImageMoveDown = true;
	}
	else if (monsterImageY >= initialMonsterImageY) {
		monsterImageMoveDown = false;
	}

	monsterImageX = monsterImageMoveForward ? monsterImageX + 0.5 : monsterImageX - 0.5;
	monsterImageY = monsterImageMoveDown ? monsterImageY + 0.5 : monsterImageY - 0.5;

	montserBonusSize = _monster1?.level;
	if (montserBonusSize === undefined) {
		montserBonusSize = 0;
	}
	else {
		montserBonusSize *= 3;
	}
	ctx.drawImage(monsterImage, monsterImageX, monsterImageY, 75 + montserBonusSize, 75 + montserBonusSize);
}

function AnimatePlayer() {
	ctx.drawImage(playerImage, canvas.width-playerImage.width, initialMonsterImageY, 100, 100);
}

function DrawMonsterStatus() {
	let monster_hp = _monster1.hp ?? 0;
	let monster_max_hp = _monster1.max_hp ?? 0;
	ctx.fillText('Health:', 10, 30);
	ctx.fillText(`${ monster_hp }/${ monster_max_hp }`, 100, 30);

	var monster_strenfth = _monster1.attack_strength ?? "0";
	ctx.fillText('Attack strength:', 10, 60);
	ctx.fillText(monster_strenfth, 200, 60);

	ctx.fillText('Debuffs:', 10, 90);

	var monster_name_y = monsterImageY + monsterImage.height + 60;
	var monster_name_x = monsterImageX;
	let monster_name = _monster1.monster_name ?? "?";
	let monsterlevel = _monster1.level ?? "?";
	ctx.fillText(`${ monster_name } lvl <${ monsterlevel }>`, monster_name_x, monster_name_y);
}

let _damage_to_show;
let _enable_damage_to_show = false;
function DrawDamage() {
	var monster_name_y = monsterImageY + monsterImage.height - 100;
	var monster_name_x = monsterImageX;
	ctx.font = "60px myFirstFont";
	ctx.fillText(`${ _damage_to_show }!`, monster_name_x, monster_name_y);
}

function CanvasShowDamageAnimation(damage) {
	_damage_to_show = damage;
	_enable_damage_to_show = true;  
	setInterval(() => {_enable_damage_to_show = false;}, 3000); // set to 5000 for crit
}

// function DebuffsString()
//     {
//         var debuffs = new List<object>();
//         if (BlindDuration > 0)
//             debuffs.Add($"blinded { BlindDuration }");

//         if (LowerAttackDuration > 0)
//             debuffs.Add($"lowered attack by { LowerAttackPercentage * 100 }% for { LowerAttackDuration } attacks");

//         if (StunDuration > 0)
//             debuffs.Add($"stunned { StunDuration }");

//         if (EasierToCritDuration > 0)
//             debuffs.Add($"easier to crit { EasierToCritPercentage }% for { EasierToCritDuration } attacks");

//         var result = "";
//         if (debuffs.Any())
//             result += "\n";

//         return result + String.Join("\n", debuffs);
//     }

// function MonsterTypesStr()
// {
//     var numberOfTypes = MonsterTypeList.Count;
//     var typesStr = "";
//     if (numberOfTypes == 1)
//     {
//         typesStr = MonsterTypeList[0].ToString();
//     }
//     else if (numberOfTypes == 2)
//     {
//         typesStr = $"{ MonsterTypeList[0] } and { MonsterTypeList[1] }";
//     }
//     else if (numberOfTypes > 2)
//     {
//         typesStr = String.Join(", ", (MonsterTypeList.Take(MonsterTypeList.Count - 1)));
//         typesStr += $", and { MonsterTypeList.Last() }";
//     }
//     return typesStr;
// }
