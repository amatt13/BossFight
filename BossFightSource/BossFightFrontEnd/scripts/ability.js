function PlayerCast(ability_name) {
    current_player_id = _player.player_id;
    if (client_side_evaluation(ability_name))
    	SendAbilityCastMessage(ability_name);
}

function client_side_evaluation(ability_name) {
    return true;
}

async function SendAbilityCastMessage(ability_name) {
	const obj = {
		request_key: "CastAbility",
		request_data: JSON.stringify({
			player_id: _player.player_id,
            ability_name: ability_name.replace(" ", "")
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

function handleAbilityCastResult(json_dict) {
	ReadPlayerMessage(json_dict["update_player"]);
	const text = json_dict["ability_text_result"];
	if (text.length > 0) {
		LogToCombatLog(text);
	}
}
