function PlayerCast(ability_name) {
    current_player_id = _player.player_id;
    client_side_evaluation(ability_name);
    SendAbilityCastMessage(ability_name);
}

function client_side_evaluation(ability_name) {
    //
}

function Heal() {

}

async function SendAbilityCastMessage(ability_name) {
	const obj = { 
		request_key: "CastAbility", 
		request_data: JSON.stringify({
			player_id: _player.player_id,
            ability_name: ability_name
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}
