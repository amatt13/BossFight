function PlayerCast(ability_cast_key) {
    current_player_id = _player.player_id;
	let abiliy_to_be_cast = null;
	_player.player_player_class.player_class.abilities.forEach(ability => {
		if (ability.ability_cast_key == ability_cast_key) {
			abiliy_to_be_cast = ability;
		}
	});

    if (client_side_evaluation(abiliy_to_be_cast)) {
		let target_id = null;
		if (abiliy_to_be_cast.only_target_monster) {
			target_id = _monster1.monster_instance_id;
		}
		else {
			target_id = _player.player_id;
		}
    	SendAbilityCastMessage(ability_cast_key, target_id);
	}
}

function client_side_evaluation(abiliy) {
	let can_cast = true;
    if (abiliy == null) {
		LogToCombatLog("Ability was not found");
		can_cast = false;
	}
	else if (abiliy.mana_cost > _player.mana) {
		LogToCombatLog("Not enough mana");
		can_cast = false;
	}
	else if (_player.hp <= 0) {
		LogToCombatLog("Can not use ability when at or below 0 HP");
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
		UpdateUiTargets(json_dict["attack_summary"]);
		const text = json_dict["ability_text_result"];
		if (text.length > 0) {
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
