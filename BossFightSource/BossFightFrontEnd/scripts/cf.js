let _player = new Player();
let _monster1 = new Monster();


function ReadPlayerMessage(playerDict_dict) {
	_player = Player.CreateFromDict(playerDict_dict);
	UpdateUiPlayerStats(_player);
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
	if (inventory_lenght < 10) {
		inventory_lenght = 10;
	}

	player_inventory.size = inventory_lenght;
}



function BlinkDiv(div_id, color = 'yellow') {
	let element = document.getElementById(div_id)
	const origcolor = element.style.backgroundColor
	if (origcolor == color)

	return;
	element.style.backgroundColor = color;
	let t = setTimeout(function () {
		element.style.backgroundColor = origcolor;
	}, (1 * 1000));
}

function NewMonster(new_monster_dict) {
	const monsterWasKilledMessage = new_monster_dict["monsterWasKilledMessage"];
	const monsterDamageInfo = new_monster_dict["monsterDamageInfo"];
	LogToGeneralLog(monsterWasKilledMessage);
	LogToGeneralLog(monsterDamageInfo);

	const newMonsterInstance_dict = new_monster_dict["newMonsterInstance"];
	const monster = Monster.CreateFromDict(newMonsterInstance_dict);
	UpdateUiActiveMonster(monster, true)
}
