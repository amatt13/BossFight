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

	var weapon_dict = json_dict["Weapon"];
	var weapon = new Weapon(weapon_dict["WeaponType"], weapon_dict["AttackMessage"], weapon_dict["BossWeapon"], weapon_dict["WeaponLvl"], weapon_dict["AttackPower"], 
		weapon_dict["AttackCritChance"], weapon_dict["SpellPower"], weapon_dict["SpellCritChance"], weapon_dict["LootId"], weapon_dict["LootName"], 
		weapon_dict["LootDropChance"], weapon_dict["Cost"]);
	
	var player_player_class_dict = json_dict["PlayerPlayerClass"];
	var player_player_class = new PlayerPlayerClass(player_player_class_dict["XP"], player_player_class_dict["Level"], player_player_class_dict["MaxHp"], player_player_class_dict["MaxMana"], player_player_class_dict["PlayerClassName"], );
	
	var player_weapon_list = [];
	var player_weapon_dict = json_dict["PlayerWeaponList"];
	player_weapon_dict.forEach(pw => {
		player_weapon_list.push(new PlayerWeapon(pw["WeaponId"], pw["WeaponName"]))
	});

	_player = new Player(json_dict["PlayerId"], json_dict["Name"], json_dict["Hp"], json_dict["Mana"], json_dict["Gold"], weapon, player_player_class, player_weapon_list);
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

// TEST Get WebSocket button
async function LoginTestUser2() {
	const obj = { 
		request_key: "FetchPlayer", 
		request_data: JSON.stringify({
			player_id: "9001"
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
	document.getElementById("player_level_and_class").innerHTML = `${ player.player_player_class.level } ${ player.player_player_class.player_class_name }`; 
	document.getElementById("player_xp").innerHTML = player.player_player_class.xp; 
	document.getElementById("player_hp").innerHTML = `${ player.hp }/${ player.player_player_class.max_hp }`;
	document.getElementById("player_mana").innerHTML = `${ player.mana }/${ player.player_player_class.max_mana }`;
	document.getElementById("player_gold").innerHTML = player.gold;
	document.getElementById("player_equipped_weapon_name").innerHTML = player.weapon.loot_name;
	
	// clear inventory, and repopulate it again
	$("#player_inventory").empty();
	var player_inventory = document.getElementById("player_inventory");
	player.player_weapon_list.forEach(player_weapon => {
		var option = document.createElement("option");
		option.text = player_weapon.name;
		option.dataset.weapon_id = player_weapon.weapon_id;
		player_inventory.add(option);
	});
	
	var inventory_lenght = player.player_weapon_list.length;
	if (inventory_lenght < 10)
	{ inventory_lenght = 10; }
	player_inventory.size = inventory_lenght;
	
	// set HP bar
	var progress_player_health = document.getElementById("progress_player_health");
	progress_player_health.max = player.player_player_class.max_hp;
	progress_player_health.value = player.hp;

	// set mana bar
	var progress_player_mana = document.getElementById("progress_player_mana");
	progress_player_mana.max = player.player_player_class.max_mana;
	progress_player_mana.value = player.mana;
}


class Player {
	constructor(player_id, name, hp, mana, gold, weapon, player_player_class, player_weapon_list) {
		this.player_id = player_id
		this.name = name;
		this.hp = hp;
		this.mana = mana;
		this.gold = gold;
		this.player_class = null;
		this.weapon = weapon
		this.player_player_class = player_player_class;
		this.player_weapon_list = player_weapon_list
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

let _player = new Player();