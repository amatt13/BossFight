let _player = new Player();
let _monster1 = new Monster();


function ReadPlayerMessage(playerDict_dict) {
	let weapon_dict = playerDict_dict["Weapon"];
	let weapon = CreateWeaponFromWeaponDict(weapon_dict);

	let player_player_class_dict = playerDict_dict["PlayerPlayerClass"];
	let player_player_class = new PlayerPlayerClass(player_player_class_dict["XP"], player_player_class_dict["Level"], player_player_class_dict["MaxHp"], player_player_class_dict["MaxMana"], player_player_class_dict["PlayerClassName"], player_player_class_dict["XpNeededToNextLevel"]);

	let player_weapon_list = [];
	let player_weapon_dict = playerDict_dict["PlayerWeaponList"];
	player_weapon_dict.forEach(pw => {
		player_weapon_list.push(new PlayerWeapon(pw["WeaponId"], pw["WeaponName"]))
	});

	_player = new Player(playerDict_dict["PlayerId"], playerDict_dict["Name"], playerDict_dict["Hp"], playerDict_dict["Mana"], playerDict_dict["Gold"], weapon, player_player_class, player_weapon_list, playerDict_dict["UserName"]);
	UpdateUiPlayerStats(_player);
}

function CreateWeaponFromWeaponDict(weapon_dict) {
	const weapon = new Weapon(weapon_dict["WeaponType"], weapon_dict["AttackMessage"], weapon_dict["BossWeapon"], weapon_dict["WeaponLvl"], weapon_dict["AttackPower"],
		weapon_dict["AttackCritChance"], weapon_dict["SpellPower"], weapon_dict["SpellCritChance"], weapon_dict["LootId"], weapon_dict["LootName"],
		weapon_dict["LootDropChance"], weapon_dict["Cost"]);
	return weapon;
}

function RepopulatePlayerInventory() {
	// clear inventory, and repopulate it again
	$("#player_inventory").empty();
	let player_inventory = document.getElementById("player_inventory");
	_player.player_weapon_list.forEach(player_weapon => {
		let option = document.createElement("option");
		option.text = player_weapon.name;
		option.dataset.weapon_id = player_weapon.weapon_id;
		player_inventory.add(option);
	});
	let inventory_lenght = _player.player_weapon_list.length;
	if (inventory_lenght < 10) { inventory_lenght = 10; }
	player_inventory.size = inventory_lenght;
}

function CreatePlayerAttackSummaryMessage(summary_dict) {
	let player_combat_log_message = ""
	const player_dict = summary_dict["Player"]
	const monster_dict = summary_dict["Monster"];
	const player_name = player_dict["Name"]  // TODO use in global (room?) combat log
	const monster_name = monster_dict["Name"]
	const player_attack_was_crit = summary_dict["PlayerCrit"]
	const player_total_damage = summary_dict["PlayerTotalDamage"]
	if (player_attack_was_crit) {
		player_combat_log_message += "Critical hit!\n"
	}
	player_combat_log_message += `You hit ${monster_name} for ${player_total_damage} damage`
	return player_combat_log_message
}

function BlinkDiv(div_id, color = 'yellow') {
	let element = document.getElementById(div_id)
	const origcolor = element.style.backgroundColor
	element.style.backgroundColor = color;
	let t = setTimeout(function () {
		element.style.backgroundColor = origcolor;
	}, (1 * 1000));
}

function CreateMonsterFromDict(monster_dict) {
	return new Monster(monster_dict["Hp"], monster_dict["MaxHp"], monster_dict["Level"], monster_dict["Name"], monster_dict["IsBossMonster"], monster_dict["MonsterInstanceId"], monster_dict["BlindDuration"],
	monster_dict["EaserToCritDuration"], monster_dict["EaserToCritPercentage"], monster_dict["LowerAttackDuration"], monster_dict["LowerAttackPercentage"], monster_dict["StunDuration"], monster_dict["AttackStrength"])
}

function CreateChatMessageFromDict(chat_message_dict) {
	return new ChatMessage(chat_message_dict["ChatMessageId"], chat_message_dict["PlayerName"], chat_message_dict["MessageContent"], Date.parse(chat_message_dict["Timestamp"]))
}

function NewMonster(new_monster_dict) {
	const monsterWasKilledMessage = new_monster_dict["monsterWasKilledMessage"];
	const monsterDamageInfo = new_monster_dict["monsterDamageInfo"];
	LogToGeneralLog(monsterWasKilledMessage);
	LogToGeneralLog(monsterDamageInfo);

	let newMonsterInstance_dict = new_monster_dict["newMonsterInstance"];
	UpdateUiActiveMonster(newMonsterInstance_dict)
}
