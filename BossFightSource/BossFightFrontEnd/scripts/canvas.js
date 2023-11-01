const canvas = document.getElementById('mainCanvas');
const _ctx = canvas.getContext('2d');
let playerImage = document.getElementById("playerSprite");
let monsterImage = document.getElementById("monsterSprite");
const voteUpButton = document.getElementById("voteMonsterTierUpButton");
const voteDownButton = document.getElementById("voteMonsterTierDownButton");
let initialMonsterImageX = 30, initialMonsterImageY = -1;
let playerWidthPlacement = 0;
let _player_info_list = [];

monsterImage.addEventListener("load", () => {
	initialMonsterImageY = canvas.height / 2;
	_ctx.drawImage(monsterImage, initialMonsterImageX, initialMonsterImageY);
});

playerImage.addEventListener("load", () => {
	_ctx.drawImage(playerImage, canvas.width-playerImage.width, initialMonsterImageY);
});

canvas.addEventListener("mouseup", function(e) {
	const x = e.pageX-$(canvas).offset().left;
	const y = e.pageY-$(canvas).offset().top;
	const playerx0 = canvas.width-playerImage.width;
	const playery0 = initialMonsterImageY;
	const playerx1 = playerx0 + 100;
	const playery1 = playery0 + 100;
	const hit = x >= playerx0 && x <= playerx1
				&& y >= playery0 && y <= playery1;
	if (hit) {
		alert(`x: ${x} y: ${y}\n${hit}`);
	}
})

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

function addPlayersToCanvas(json_player_info_list) {
	_player_info_list = [];
	json_player_info_list.forEach(info_dict => {
		_player_info_list.push(PlayerInformation.CreateFromDict(info_dict));
	});
	populateOtherPlayersWindow(_player_info_list);
}

function getCircleX(radians, radius) {
	return Math.cos(radians) * radius;
  }

  function getCircleY(radians, radius) {
	return Math.sin(radians) * radius;
  }

  function degreesToRad(deg) {
	return deg * Math.PI / 180;
  }

  function radToDegrees(rad) {
	return rad * 180 / Math.PI;
  }

// Only drawing stuff below
window.requestAnimationFrame(Draw);

function Draw() {
	ReSizeCanvas();  // MUST be the first thing that happens
	_ctx.fillStyle = "hsl(349, 19%, 45%)"
	ResetFont();
	_ctx.globalCompositeOperation = 'destination-over';
	_ctx.clearRect(0, 0, canvas.width, canvas.height); // clear canvas
	AnimateMonster();
	DrawMonsterStatus();
	AnimatePlayer();
	AnimateOtherPlayers();
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
	_ctx.font = "30px myFirstFont";
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

	_ctx.drawImage(monsterImage, monsterImageX, monsterImageY, 160, 160);

	var monster_name_y = monsterImageY - 5;// - monsterImage.naturalHeight;
	var monster_name_x = monsterImageX;
	let monster_name = _monster1.monster_name ?? "?";
	let monsterlevel = _monster1.level ?? "?";
	_ctx.fillText(`${ monster_name } level ${ monsterlevel }`, monster_name_x, monster_name_y);
}

function AnimatePlayer() {
	const player_x = canvas.width-playerImage.width;
	const player_y = initialMonsterImageY;
	_ctx.drawImage(playerImage, player_x, player_y, 100, 100);
}

function AnimateOtherPlayers() {
	_ctx.font = "15px myFirstFont";

	const number_of_players_to_draw = _player_info_list.length;
	if (number_of_players_to_draw > 0) {
		const r = 150;
		const circle_start = -90;
		const circle_end = -270;
		const available_degrees = circle_end - circle_start;
		const move_distance = available_degrees / number_of_players_to_draw;

		const player_x = canvas.width-playerImage.width;
		const player_y = initialMonsterImageY;

		for (let index = 0; index < number_of_players_to_draw; index++) {
			const degs = circle_start + move_distance * (index + 1) - move_distance / 2;
			const rads = degreesToRad(degs)
			const x_rads = Math.round(getCircleX(rads, r) * 100) / 100 + (player_x);
			const y_rads = Math.round(getCircleY(rads, r) * 100) / 100 + (player_y);

			const other_player = _player_info_list[index];
			_ctx.drawImage(other_player.image_source, x_rads, y_rads, 100, 100);

			_ctx.fillText(`${ other_player.name } level ${ other_player.level }`, x_rads, y_rads + 100);
			_ctx.fillText(`${ other_player.current_hp }/${ other_player.max_hp } HP - ${ other_player.current_mana }/${ other_player.max_mana } mana`, x_rads, y_rads + 115);
		}

		// The great debug circle that shows where other players should be shown on the canvas
		// ctx.beginPath();
		// ctx.arc(player_x + playerImage.width / 3, player_y + playerImage.height / 3, 200, 0, 2 * Math.PI);
		// ctx.stroke();

		ResetFont();
	}
}

function DrawMonsterStatus() {
	let monster_hp = _monster1.hp ?? 0;
	let monster_max_hp = _monster1.max_hp ?? 0;
	let = monster_type_list = [];
	if (_monster1.monster_template != undefined && _monster1.monster_template.monster_type_list != undefined) {
		monster_type_list = _monster1.monster_template.monster_type_list ?? [""]
	}
	_ctx.fillText(`${ _monster1.monster_name } <${ monster_type_list.join(' ') }>`, 10, 30)

	_ctx.fillText('Health:', 10, 60);
	_ctx.fillText(`${ monster_hp }/${ monster_max_hp }`, 100, 60);

	var monster_strenfth = _monster1.attack_strength ?? "0";
	_ctx.fillText('Attack strength:', 10, 90);
	_ctx.fillText(monster_strenfth, 200, 90);

	_ctx.fillText('Active Effects:', 10, 120);
}

let _damage_to_show;
let _enable_damage_to_show = false;
function DrawDamage() {
	var monster_name_y = monsterImageY + monsterImage.height - 100;
	var monster_name_x = monsterImageX;
	_ctx.font = "60px myFirstFont";
	_ctx.fillText(`${ _damage_to_show }!`, monster_name_x, monster_name_y);
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
	_ctx.font = "60px myFirstFont";
	_ctx.fillText(`${ _damage_to_showPlayer }!`, player_x, Player_y);
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
