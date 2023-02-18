function UpdateUiActiveMonster(monster_dict, monster_is_new = false) {
	_monster1 = CreateMonsterFromDict(monster_dict);
	document.getElementById("monsterSprite").src = `./images/sprites/monsters/${_monster1.monster_name[0].toLowerCase() + _monster1.monster_name.substr(1, _monster1.monster_name.length-1).replaceAll(" ", "")}.png`
	if (monster_is_new ) {
		voteUpButton.classList.remove("highligtedButton")
		voteDownButton.classList.remove("highligtedButton")
	}
}

function UpdateUiPlayerStats(player) {
	document.getElementById("player_name").innerHTML = player.name;
	document.getElementById("player_level_and_class").innerHTML = `${numberToString(player.player_player_class.level)} ${player.player_player_class.player_class_name}`;
	document.getElementById("player_xp").innerHTML = numberToString(player.player_player_class.xp);
	document.getElementById("player_xp_to_next_level").innerHTML = `(${numberToString(player.player_player_class.xp_to_next_level)} to next level)`
	document.getElementById("player_hp").innerHTML = `${numberToString(player.hp)}/${numberToString(player.player_player_class.max_hp)}`;
	document.getElementById("player_mana").innerHTML = `${numberToString(player.mana)}/${numberToString(player.player_player_class.max_mana)}`;
	document.getElementById("player_gold").innerHTML = numberToString(_player.gold);
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
	document.getElementById("player_gold").innerHTML = numberToString(_player.gold);  //TODO use bindings instead of manually setting the text whenever something changes?
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

function UpdateMonsterTierVoteBasedOnCurrentPlayerVote(vote_dict) {
	const monster_instance_id = vote_dict["MonsterInstanceId"]
	const vote =  vote_dict["Vote"]
	if (monster_instance_id != undefined && vote != undefined)
	{
		voteUpButton.classList.remove("highligtedButton")
		voteDownButton.classList.remove("highligtedButton")

		switch (vote) {
			case MonsterTierVoteChoice.DECREASE_DIFFICULTY:
				voteDownButton.classList.add("highligtedButton")
				break;
			case MonsterTierVoteChoice.INCREASE_DIFFICULTY:
				voteUpButton.classList.add("highligtedButton")
				break;
			default:
				break;
		}
	}
}

function CreatePlayerclassTitleCardForShop(playerclass, row) {
	const card_html = `<div style="border: solid; border-color: var(--border-colour); margin-left: 5px">
		<table>
			<tr>
				<td style="width: 50%; float: left">
					<img id="shop_menu_player_class${ playerclass.name }_sprite" src="./images/sprites/player_classes/${ playerclass.name }.png" width="75" height="75" style="object-fit: fill;">
				</td>
				<td>
					<label>${ playerclass.description }</label>
				</td>
			</tr>
			<tr>
				<td>
					<label>${ playerclass.name.toLowerCase() } - cost ${ numberToString(playerclass.purchase_price) }</label>
				</td>
			</tr>
			<tr>
				<td>
					<label>Base health: ${ numberToString(playerclass.base_health) }</label>
				</td>
			</tr>
			<tr>
				<td>
					<label>Base mana: ${ numberToString(playerclass.base_mana) }</label>
				</td>
			</tr>
		</table>
	</div>`

	return card_html;
}

let _playerclasses_list = new Array();

function UpdateUiShop(shop_dict) {
	const player_classes = shop_dict["playerClasses"];
	let player_class_html = "";
	_playerclasses_list = new Array();

	player_classes.forEach((player_class, i) => {
		const playerclass_count = i + 2;
		const aquired = player_class["Aquired"];
		const pc = PlayerClass.CreateFromDict(player_class["PlayerClass"]);
		_playerclasses_list.push(pc);
		const card = CreatePlayerclassTitleCardForShop(pc, playerclass_count);
		const class_row = `<div id="PlayerClassContainer" style="grid-column: 2; grid-row: ${ playerclass_count };">
		${ card }
		<button class="buy-button btn-primary" ${ aquired ? "disabled" : "" } onclick="BuyPlayerClass(${ pc.player_class_id });">${ aquired ? "Already owned" : "Buy" }</button>
	</div>`;
		player_class_html += class_row;
	});

	document.getElementById("playerclass_buy_options").innerHTML = player_class_html;
}
