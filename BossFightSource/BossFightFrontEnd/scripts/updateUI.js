function UpdateUiActiveMonster(monster, monster_is_new = false) {
	_monster1 = monster;
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
	let progress_player_health = document.getElementById("progress_player_health");
	progress_player_health.max = player.player_player_class.max_hp;
	progress_player_health.value = player.hp;

	// set mana bar
	let progress_player_mana = document.getElementById("progress_player_mana");
	progress_player_mana.max = player.player_player_class.max_mana;
	progress_player_mana.value = player.mana;

	// set player sprite
	const current_name = document.getElementById("playerSprite").src;
	const class_name = player.player_player_class.player_class_name.toLowerCase();
	const preffered_body_type = player.preffered_body_type.Name;
	const sprite_name = buildPlayerSpriteName(class_name, preffered_body_type, true);
	if (sprite_name != current_name.slice(current_name.length-sprite_name.length, current_name.length)) {  // only set the source anew if the class changed
		const sprite_source = getPlayerClassSprite(class_name, preffered_body_type);
		document.getElementById("playerSprite").src = sprite_source.src;
	}

	// set abilities
	let ability_list = document.getElementById("player_abilities_list");
	new_abilities_list = "";
	player.player_player_class.player_class.abilities.forEach(ability => {
		ability_html = CreateAbilityForPlayerColumn(ability);
		new_abilities_list += ability_html;
	});
	ability_list.innerHTML = new_abilities_list;
	document.querySelectorAll('[id^=button_ability_]').forEach(ability_button => {
		ability_button.addEventListener('contextmenu', function (event) {
			event.preventDefault();
			showContextMenu(event);
		});
	})
}

function UpdateUiPlayerSoldWeapon(json_dict) {
	const gold = json_dict["gold"]
	_player.gold = gold;
	document.getElementById("player_gold").innerHTML = numberToString(_player.gold);  //TODO use bindings instead of manually setting the text whenever something changes?
	BlinkDiv("player_gold");

	let player_weapon_list = [];
	let player_weapon_dict = json_dict["weapons"];
	player_weapon_dict.forEach(pw => {
		player_weapon_list.push(new PlayerWeapon(pw["WeaponId"], pw["WeaponName"]))
	});
	_player.player_weapon_list = player_weapon_list;
	RepopulatePlayerInventory();
}

function UpdateUiPlayerEquippedWeapon(weapon_dict) {
	const weapon = Weapon.CreateFromDict(weapon_dict);
	_player.weapon = weapon;
	document.getElementById("player_equipped_weapon_name").innerHTML = _player.weapon.loot_name;
	BlinkDiv("player_equipped_weapon_name");
}

function UpdateUiPlayerAttackedMonsterWithWeapon(summary) {
	UpdateUiActiveMonster(summary.monster);
	_player = summary.player;
	UpdateUiPlayerStats(summary.player);
	BlinkDiv("player_xp");
	player_combat_log_message = CreateAttackSummaryMessage(summary)
	LogToCombatLog(player_combat_log_message);
	if (summary.monster_retaliate_message != null && summary.monster_retaliate_message.length > 0) {
		LogToCombatLog(summary.monster_retaliate_message);
	}
	CanvasShowDamageAnimationForPlayer(summary.monster_total_damage);
	CanvasShowDamageAnimation(summary.player_total_damage);
}

function CreateAttackSummaryMessage(summary) {
	let player_combat_log_message = ""

	if (summary.player_crit) {
		player_combat_log_message += "Critical hit!\n"
	}

	let hit_message = "You hit";
	if (summary.player.weapon.attack_message != null && summary.player.weapon.attack_message != "") {
		hit_message = summary.player.weapon.attack_message;
		// if the last char is NOT '.', '?', or '!'
		if (!/[.?!]/.test(hit_message.slice(-1))) {
			hit_message += "."
		}
	} // recreate the function to use the same in the future

	player_combat_log_message += `${hit_message} ${summary.monster.monster_name} takes ${summary.player_total_damage} damage`;
	return player_combat_log_message
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
	const sprite_source = getPlayerClassSprite(playerclass.name);
	const card_html = `<div style="border: solid; border-color: var(--border-colour); margin-left: 5px">
		<table>
			<tr>
				<td style="width: 50%; float: left">
					<img id="shop_menu_player_class${ playerclass.name }_sprite" src="${ sprite_source.src }" width="75" height="75" style="object-fit: fill;">
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
		const class_row = `<div style="grid-column: 2; grid-row: ${ playerclass_count };">
		${ card }
		<button class="buy-button btn-primary" ${ aquired ? "disabled" : "" } onclick="BuyPlayerClass(${ pc.player_class_id });">${ aquired ? "Already owned" : "Buy" }</button>
	</div>`;
		player_class_html += class_row;
	});

	document.getElementById("playerclass_buy_options").innerHTML = player_class_html;
}

function CreateAbilityForPlayerColumn(ability) {
	const ability_html = `<button id="button_ability_${ability.name}" type="button" class="btn-ability toolTip" onclick="player_cast('${ability.ability_cast_key}', ${_player.player_id});">
	<img src="./images/ui_icons/abilities/${ability.image_source}.png" class="max-size-100-percent" data-ability_name="${ability.name}">
	<span class="toolTipText" onclick="event.stopPropagation();">${ability.description}
		Costs ${ability.mana_cost} Mana
	</span>
</button>`

return ability_html;
}

function regenPlayerHealthAndMana(regen_health_and_mana_dict) {
	const hp = regen_health_and_mana_dict["health"];
	const mana = regen_health_and_mana_dict["mana"];

	_player.hp += hp;
	_player.mana += mana;

	// set HP bar
	let progress_player_health = document.getElementById("progress_player_health");
	progress_player_health.value = _player.hp;
	document.getElementById("player_hp").innerHTML = `${numberToString(_player.hp)}/${numberToString(_player.player_player_class.max_hp)}`;

	// set mana bar
	let progress_player_mana = document.getElementById("progress_player_mana");
	progress_player_mana.value = _player.mana;
	document.getElementById("player_mana").innerHTML = `${numberToString(_player.mana)}/${numberToString(_player.player_player_class.max_mana)}`;
}
