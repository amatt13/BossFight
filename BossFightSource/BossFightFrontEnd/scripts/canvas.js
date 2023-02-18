const canvas = document.getElementById('mainCanvas');
const ctx = canvas.getContext('2d');
let playerImage = document.getElementById("playerSprite");
let monsterImage = document.getElementById("monsterSprite");
const voteUpButton = document.getElementById("voteMonsterTierUpButton");
const voteDownButton = document.getElementById("voteMonsterTierDownButton");
let initialMonsterImageX = 30, initialMonsterImageY = -1;
let playerWidthPlacement = 0;

monsterImage.addEventListener("load", () => {
	initialMonsterImageY = canvas.height / 2;
	ctx.drawImage(monsterImage, initialMonsterImageX, initialMonsterImageY);
});

playerImage.addEventListener("load", () => {
	ctx.drawImage(playerImage, canvas.width-playerImage.width, initialMonsterImageY);
});

function Vote(vote) {
	if (_player != undefined) {
		if (_monster1 != undefined) {
			const obj = {
				//TODO validate that the user is who they pretend to be (server-side)
				request_key: "VoteForMonsterTier",
				request_data: JSON.stringify({
					"player_id": _player.player_id,
					"monster_instance_id": _monster1.monster_instance_id,
					"vote": vote
				})
			};
			const json_obj = JSON.stringify(obj);
			socket.send(json_obj);
			FetchMonsterVotesTotals();
		}
		else {
			LogToGeneralLog("Can't vote without a active monster", true)
		}
    }
    else {
        LogToGeneralLog("You are not logged in", true)
    }
}

voteUpButton.addEventListener("click", () => {
	voteUpButton.classList.add("highligtedButton")
	voteDownButton.classList.remove("highligtedButton")
	Vote(1)
});

voteDownButton.addEventListener("click", () => {
	voteDownButton.classList.add("highligtedButton")
	voteUpButton.classList.remove("highligtedButton")
	Vote(-1)
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
	WriteToVariables();
	if (_enable_damage_to_show) {
		DrawDamage();
		DrawDamageOnPlayer();
	}

	window.requestAnimationFrame(Draw);
}

function WriteToVariables(){
	playerWidthPlacement = canvas.width-playerImage.width;
}

function ReSizeCanvas() {
	if (canvas.width != Math.floor(window.innerWidth * 0.33) || canvas.height != Math.floor(window.innerHeight * 0.80)) {
		canvas.width = Math.floor(window.innerWidth * 0.33);
		canvas.height = Math.floor(window.innerHeight * 0.80);
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
	ctx.drawImage(monsterImage, monsterImageX, monsterImageY, 100 + montserBonusSize, 100 + montserBonusSize);
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

	var monster_name_y = monsterImageY + monsterImage.height + 30;
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

let _damage_to_showPlayer;
function DrawDamageOnPlayer(){
	var Player_y = initialMonsterImageY - 20;
	var player_x = playerWidthPlacement + 72 / 2 - 5;
	ctx.font = "60px myFirstFont";
	ctx.fillText(`${ _damage_to_showPlayer }!`, player_x, Player_y);
}

function CanvasShowDamageAnimationForPlayer(damage) {
	_damage_to_showPlayer = damage;
	_enable_damage_to_show = true;  
	setInterval(() => {_enable_damage_to_show = false;}, 3000); // set to 5000 for crit
}

function UpdateCanvasMonsterTierVotesTotal(monster_tier_votes_total) {
	up_votes = parseInt(monster_tier_votes_total.UpVotes);
	down_votes = parseInt(monster_tier_votes_total.DownVotes);
	vote_up_text = "<img src=\"./images/ui_icons/green_arrow_up.png\" class=\"vote-button\"> Difficulty up";
	vote_down_text = "<img src=\"./images/ui_icons/red_arrow_down.png\" class=\"vote-button\"> Difficulty down";

	if (up_votes > 0) {
		vote_up_text += ` (${up_votes})`;
	}

	if (down_votes > 0) {
		vote_down_text += ` (${down_votes})`;
	}

	voteUpButton.innerHTML = vote_up_text;
	voteDownButton.innerHTML = vote_down_text;
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
