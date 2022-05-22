function UpdateUiActiveMonster(monster_dict) {
	_monster1 = CreateMonsterFromDict(monster_dict);
	document.getElementById("monsterSprite").src = `./images/sprites/monsters/${_monster1.monster_name[0].toLowerCase() + _monster1.monster_name.substr(1, _monster1.monster_name.length-1).replaceAll(" ", "")}.png`
}

function UpdateUiPlayerStats(player) {
	document.getElementById("player_name").innerHTML = player.name;
	document.getElementById("player_level_and_class").innerHTML = `${player.player_player_class.level} ${player.player_player_class.player_class_name}`;
	document.getElementById("player_xp").innerHTML = player.player_player_class.xp;
	document.getElementById("player_xp_to_next_level").innerHTML = `(${player.player_player_class.xp_to_next_level} to next level)`
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
	document.getElementById("playerSprite").src = `./images/sprites/player_classes/${player.player_player_class.player_class_name.toLowerCase()}.png`
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

function UpdateUiPlayerEquippedWeapon(weapon_dict) {
	var weapon = CreateWeaponFromWeaponDict(weapon_dict);
	_player.weapon = weapon;
	document.getElementById("player_equipped_weapon_name").innerHTML = _player.weapon.loot_name;
	BlinkDiv("player_equipped_weapon_name");
}

function UpdateUiPlayerAttackedMonsterWithWeapon(summary_dict) {
	var monster_dict = summary_dict["Monster"];
	UpdateUiActiveMonster(monster_dict);
	var player_dict = summary_dict["Player"];
	ReadPlayerMessage(player_dict);
	BlinkDiv("player_xp");
	player_combat_log_message = CreatePlayerAttackSummaryMessage(summary_dict)
	LogToCombatLog(player_combat_log_message);
	var monster_message = summary_dict["MonsterRetaliateMessage"];
	if (monster_message != undefined && monster_message.length > 0) {
		LogToCombatLog(monster_message);
	}
	CanvasShowDamageAnimation(summary_dict["PlayerTotalDamage"]);
}
