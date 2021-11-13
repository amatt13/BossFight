const canvas = document.getElementById('mainCanvas');
const ctx = canvas.getContext('2d');
var monsterImage = new Image();
monsterImage.src = "images/sprites/monsters/goblin.png";
let initialMonsterImageX = 30, initialMMonsterImageY = -1;

monsterImage.addEventListener("load", () => {
	initialMMonsterImageY += monsterImage.height;
	ctx.drawImage(monsterImage, initialMonsterImageX, initialMMonsterImageY);
});


let socket = new WebSocket("ws://localhost:5000/ws");

socket.onopen = function (e) {
	LogToTextLog("[open] Connection established")
	FetchActiveMonster();
};

socket.onmessage = function (event) {
	//LogToTextLog(`[message] Data received from server: ${event.data}`);
	var json_dict = JSON.parse(event.data);

	if ("fetch_active_monster" in json_dict)
		UpdateUiActiveMonster(json_dict["fetch_active_monster"]);
	else if ("update_player" in json_dict)
		ReadPlayerMessage(json_dict["update_player"]);
	else if ("update_player_sold_weapon" in json_dict)
		UpdateUiPlayerSoldWeapon(json_dict["update_player_sold_weapon"])
	else if ("update_player_equipped_weapon" in json_dict)
		UpdateUiPlayerEquippedWeapon(json_dict["update_player_equipped_weapon"])
	else if ("player_attacked_monster_with_weapon" in json_dict)
		UpdateUiPlayerAttackedMonsterWithWeapon(json_dict["player_attacked_monster_with_weapon"])
	else if ("error_message" in json_dict)
		LogToTextLog(json_dict["error_message"], true)
};

socket.onclose = function (event) {
	if (event.wasClean) {
		LogToTextLog(`[close] Connection closed cleanly, code=${event.code} reason=${event.reason}`);
	}
	else {
		LogToTextLog('[close] Connection died');
	}
};

socket.onerror = function (error) {
	LogToTextLog(`[error] ${error.message}`);
};

async function FetchActiveMonster() {
	const obj = {
		request_key: "FetchActiveMonster",
		request_data: JSON.stringify({
			player_id: _player.player_id
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

// TEST Get WebSocket button
async function LoginTestUser() {
	const obj = {
		request_key: "FetchPlayer",
		request_data: JSON.stringify({
			player_id: "1337"
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

// TEST Get WebSocket button
async function LoginTestUser2() {
	const obj = {
		request_key: "FetchPlayer",
		request_data: JSON.stringify({
			player_id: "9001"
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

async function AttackMonster() {
	const obj = {
		request_key: "PlayerAttackMonsterWithEquippedWeapon",
		request_data: JSON.stringify({
			player_id: _player.player_id
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}


function LogToTextLog(pText, blink = false) {
	document.getElementById("text_log").innerHTML += pText + '\n'
	if (blink) {
		BlinkDiv("text_log", 'red');
	}
}

function ReadPlayerMessage(json_dict) {
	var weapon_dict = json_dict["Weapon"];
	var weapon = CreateWeaponFromWeaponDict(weapon_dict);

	var player_player_class_dict = json_dict["PlayerPlayerClass"];
	var player_player_class = new PlayerPlayerClass(player_player_class_dict["XP"], player_player_class_dict["Level"], player_player_class_dict["MaxHp"], player_player_class_dict["MaxMana"], player_player_class_dict["PlayerClassName"],);

	var player_weapon_list = [];
	var player_weapon_dict = json_dict["PlayerWeaponList"];
	player_weapon_dict.forEach(pw => {
		player_weapon_list.push(new PlayerWeapon(pw["WeaponId"], pw["WeaponName"]))
	});

	_player = new Player(json_dict["PlayerId"], json_dict["Name"], json_dict["Hp"], json_dict["Mana"], json_dict["Gold"], weapon, player_player_class, player_weapon_list, json_dict["UserName"]);
	UpdateUiPlayerStats(_player);
}

function CreateWeaponFromWeaponDict(weapon_dict) {
	let weapon = new Weapon(weapon_dict["WeaponType"], weapon_dict["AttackMessage"], weapon_dict["BossWeapon"], weapon_dict["WeaponLvl"], weapon_dict["AttackPower"],
		weapon_dict["AttackCritChance"], weapon_dict["SpellPower"], weapon_dict["SpellCritChance"], weapon_dict["LootId"], weapon_dict["LootName"],
		weapon_dict["LootDropChance"], weapon_dict["Cost"]);
	return weapon;
}

function UpdateUiActiveMonster(monster_dict) {
	_monster1 = new Monster(monster_dict["Hp"], monster_dict["MaxHp"], monster_dict["Level"], monster_dict["Name"], monster_dict["IsBossMonster"], monster_dict["MonsterInstanceId"], monster_dict["BlindDuration"],
		monster_dict["EaserToCritDuration"], monster_dict["EaserToCritPercentage"], monster_dict["LowerAttackDuration"], monster_dict["LowerAttackPercentage"], monster_dict["StunDuration"], monster_dict["AttackStrength"])
}

function UpdateUiPlayerStats(player) {
	document.getElementById("player_name").innerHTML = player.name;
	document.getElementById("player_level_and_class").innerHTML = `${player.player_player_class.level} ${player.player_player_class.player_class_name}`;
	document.getElementById("player_xp").innerHTML = player.player_player_class.xp;
	document.getElementById("player_hp").innerHTML = `${player.hp}/${player.player_player_class.max_hp}`;
	document.getElementById("player_mana").innerHTML = `${player.mana}/${player.player_player_class.max_mana}`;
	document.getElementById("player_gold").innerHTML = _player.gold;
	document.getElementById("player_equipped_weapon_name").innerHTML = player.weapon.loot_name;

	RepopulatePlayerInventory();

	// set HP bar
	var progress_player_health = document.getElementById("progress_player_health");
	progress_player_health.max = player.player_player_class.max_hp;
	progress_player_health.value = player.hp;

	// set mana bar
	var progress_player_mana = document.getElementById("progress_player_mana");
	progress_player_mana.max = player.player_player_class.max_mana;
	progress_player_mana.value = player.mana;

	// set player sprite
	document.getElementById("playerSprite").src = `./images/sprites/player_classes/${player.player_player_class.player_class_name}.png`
}

function RepopulatePlayerInventory() {
	// clear inventory, and repopulate it again
	$("#player_inventory").empty();
	var player_inventory = document.getElementById("player_inventory");
	_player.player_weapon_list.forEach(player_weapon => {
		var option = document.createElement("option");
		option.text = player_weapon.name;
		option.dataset.weapon_id = player_weapon.weapon_id;
		player_inventory.add(option);
	});
	var inventory_lenght = _player.player_weapon_list.length;
	if (inventory_lenght < 10) { inventory_lenght = 10; }
	player_inventory.size = inventory_lenght;
}

function UpdateUiPlayerSoldWeapon(json_dict) {
	const gold = json_dict["gold"]
	_player.gold = gold;
	document.getElementById("player_gold").innerHTML = _player.gold;
	BlinkDiv("player_gold");

	var player_weapon_list = [];
	var player_weapon_dict = json_dict["weapons"];
	player_weapon_dict.forEach(pw => {
		player_weapon_list.push(new PlayerWeapon(pw["WeaponId"], pw["WeaponName"]))
	});
	_player.player_weapon_list = player_weapon_list;
	RepopulatePlayerInventory();
}

function UpdateUiPlayerEquippedWeapon(json_dict) {
	var weapon_dict = json_dict["Weapon"];
	var weapon = CreateWeaponFromWeaponDict(weapon_dict);
	_player.weapon = weapon;
	document.getElementById("player_equipped_weapon_name").innerHTML = _player.weapon.loot_name;
	BlinkDiv("player_equipped_weapon_name");
}

function UpdateUiPlayerAttackedMonsterWithWeapon(json_dict) {
	var summary_dict = json_dict["summary"];
	var monster_dict = summary_dict["Monster"];
	_monster1 = new Monster(monster_dict["Hp"], monster_dict["MaxHp"], monster_dict["Level"], monster_dict["Name"], monster_dict["IsBossMonster"], monster_dict["MonsterInstanceId"], monster_dict["BlindDuration"],
		monster_dict["EaserToCritDuration"], monster_dict["EaserToCritPercentage"], monster_dict["LowerAttackDuration"], monster_dict["LowerAttackPercentage"], monster_dict["StunDuration"], monster_dict["AttackStrength"])
}

function BlinkDiv(div_id, color = 'yellow') {
	var element = document.getElementById(div_id)
	var origcolor = element.style.backgroundColor
	element.style.backgroundColor = color;
	var t = setTimeout(function () {
		element.style.backgroundColor = origcolor;
	}, (1 * 1000));
}

class Player {
	constructor(player_id, name, hp, mana, gold, weapon, player_player_class, player_weapon_list, user_name) {
		this.player_id = player_id
		this.name = name;
		this.hp = hp;
		this.mana = mana;
		this.gold = gold;
		this.player_class = null;
		this.weapon = weapon
		this.player_player_class = player_player_class;
		this.player_weapon_list = player_weapon_list
		this.user_name = user_name;
	}
}

class Weapon {
	constructor(WeaponType, AttackMessage, BossWeapon, WeaponLvl, AttackPower, AttackCritChance, SpellPower, SpellCritChance, LootId, LootName, LootDropChance, Cost) {
		this.weapon_type = WeaponType
		this.attack_message = AttackMessage
		this.boss_weapon = BossWeapon
		this.weapon_lvl = WeaponLvl
		this.attack_power = AttackPower
		this.attack_crit_chance = AttackCritChance
		this.spell_power = SpellPower
		this.spell_crit_chance = SpellCritChance
		this.loot_id = LootId
		this.loot_name = LootName
		this.loot_drop_chance = LootDropChance
		this.cost = Cost
	}
}

class PlayerPlayerClass {
	constructor(xp, level, max_hp, max_mana, player_class_name) {
		this.xp = xp;
		this.level = level;
		this.max_hp = max_hp;
		this.max_mana = max_mana;
		this.player_class_name = player_class_name;
	}
}

class PlayerWeapon {
	constructor(weapon_id, name) {
		this.weapon_id = weapon_id;
		this.name = name;
	}
}

class Monster {
	constructor(hp, max_hp, level, monster_name, is_boss_monster, monster_instance_id, blind_duration, easier_to_crit_duration, easier_to_crit_Percentage, lower_attack_duration, lower_attack_percentage, stun_duration, attack_strength) {
		this.hp = hp;
		this.max_hp = max_hp;
		this.level = level;
		this.monster_name = monster_name;
		this.is_boss_monster = is_boss_monster;
		this.monster_instance_id = monster_instance_id;
		this.blind_duration = blind_duration;
		this.easier_to_crit_duration = easier_to_crit_duration;
		this.easier_to_crit_Percentage = easier_to_crit_Percentage;
		this.lower_attack_duration = lower_attack_duration;
		this.lower_attack_percentage = lower_attack_percentage;
		this.stun_duration = stun_duration;
		this.attack_strength = attack_strength;
	}
}

let _player = new Player();
let _monster1 = new Monster();
