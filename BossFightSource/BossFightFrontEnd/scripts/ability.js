function player_cast(ability_cast_key, target_id) {
	let abiliy_to_be_cast = null;
	_player.player_player_class.player_class.abilities.forEach(ability => {
		if (ability.ability_cast_key == ability_cast_key) {
			abiliy_to_be_cast = ability;
		}
	});

	if (abiliy_to_be_cast.only_target_monster)
		target_id = _monster1.monster_instance_id;

    if (client_side_evaluation(abiliy_to_be_cast, target_id)) {
    	SendAbilityCastMessage(ability_cast_key, parseInt(target_id));
	}
}

function client_side_evaluation(abiliy, target_id) {
	let can_cast = true;
    if (abiliy == null) {
		LogToCombatLog("Ability was not found");
		can_cast = false;
	}

	if (abiliy.mana_cost > _player.mana) {
		LogToCombatLog("Not enough mana");
		can_cast = false;
	}

	if (_player.hp <= 0) {
		LogToCombatLog("Can not use ability when at or below 0 HP");
		can_cast = false;
	}

	if (target_id == null || target_id == undefined) {
		LogToCombatLog("No valid target selected");
		can_cast = false;
	}

	return can_cast;
}

async function SendAbilityCastMessage(ability_cast_key, target_id) {
	const obj = {
		request_key: "CastAbility",
		request_data: JSON.stringify({
			player_id: _player.player_id,
            ability_name: ability_cast_key,
			target_id: target_id
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

function handleAbilityCastResult(json_dict) {
	const cast_success = json_dict["cast_success"];
	if (cast_success) {
		ReadPlayerMessage(json_dict["update_player"]);
		const summary_dict = json_dict["attack_summary"];
		if (summary_dict != null) {
			UpdateUiTargets(summary_dict);
		}
		const text = json_dict["ability_text_result"];
		if (text != null && text.length > 0) {
			LogToCombatLog(text);
		}
	}
	else {
		const text = json_dict["ability_text_result"];
		if (text.length > 0) {
			LogToCombatLog(text, true);
		}
	}
}

function otherPlayerCastAbilityOnCurrentPlayer(message_dict) {
	const player_caster = Player.CreateFromDict(message_dict["caster_player"]);
	const player_updated = Player.CreateFromDict(message_dict["update_player"]);
	UpdateUiPlayerStats(player_updated);
	const ability_message = message_dict["ability_text_result"];
	LogToCombatLog(ability_message, true);
}
