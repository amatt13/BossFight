let socket = undefined;
let conn_string = "";

if (location.hostname === "localhost" || location.hostname === "127.0.0.1") {
	conn_string = "ws://localhost:5000/Echo";
	console.log("unsecure localhost");
}
else {
	console.log("secure remote host");
	conn_string = "wss://bossfight.ix.tc:5000/Echo";
}
//socket = new WebSocket("wss://bossfight.ix.tc:5000/Echo");

socket = new WebSocket(conn_string);

socket.onopen = function (e) {
	LogToGeneralLog("[open] Connection established");
	FetchMostRecentMessages();
	FetchMonsterVotesTotals();
	FetchActiveMonster();
};


socket.onmessage = function (event) {
	var json_dict = JSON.parse(event.data);

	if ("fetch_active_monster" in json_dict) {
		const monster = Monster.CreateFromDict(json_dict["fetch_active_monster"]);
		UpdateUiActiveMonster(monster);
	}
	else if ("update_player" in json_dict)
		ReadPlayerMessage(json_dict["update_player"]);
	else if ("update_player_sold_weapon" in json_dict)
		UpdateUiPlayerSoldWeapon(json_dict["update_player_sold_weapon"])
	else if ("update_player_equipped_weapon" in json_dict)
		UpdateUiPlayerEquippedWeapon(json_dict["update_player_equipped_weapon"])
	else if ("player_attacked_monster_with_weapon" in json_dict)
		UpdateUiTargets(json_dict["player_attacked_monster_with_weapon"])
	else if ("receive_chat_message" in json_dict)
        ReceiveChatMessage(json_dict["receive_chat_message"]);
    else if ("receive_multiple_chat_message" in json_dict)
        PopulateChatLogWithMultipleMessages(json_dict["receive_multiple_chat_message"]);
	else if ("new_monster" in json_dict)
		NewMonster(json_dict["new_monster"]);
	else if ("player_signed_in" in json_dict) {
		ReadPlayerMessage(json_dict["player_signed_in"]["player"]);
		if (json_dict["player_signed_in"]["current_vote"] != null) {
			UpdateMonsterTierVoteBasedOnCurrentPlayerVote(json_dict["player_signed_in"]["current_vote"]);
		}
	}
	else if ("monster_tier_votes_total" in json_dict) {
		UpdateCanvasMonsterTierVotesTotal(json_dict["monster_tier_votes_total"]);
	}
	else if ("shopMenu" in json_dict) {
		UpdateUiShop(json_dict["shopMenu"]);
	}
	else if ("bought_player_class" in json_dict) {
		playerAttemptedToBuyAPlayerClass(json_dict["bought_player_class"])
	}
	else if ("unlocked_classes" in json_dict)
		showPlayerClassesMenu(json_dict["unlocked_classes"]);
	else if ("ability_cast_result" in json_dict) {
		handleAbilityCastResult(json_dict["ability_cast_result"])
	}
	else if ("regen_health_and_mana" in json_dict) {
		regenPlayerHealthAndMana(json_dict["regen_health_and_mana"]);
	}
	else if ("other_players_info_updates" in json_dict) {
		addPlayersToCanvas(json_dict["other_players_info_updates"]);
	}
	else if ("error_message" in json_dict) {
		error_message = json_dict["error_message"]
		show_custom_alert(error_message, "alarm")
		LogToGeneralLog(error_message, true);
	}
	else
		LogToGeneralLog(`Unkown message received '${JSON.stringify(json_dict)}'`, true);

};

socket.onclose = function (event) {
	if (event.wasClean) {
		LogToGeneralLog(`[close] Connection closed cleanly, code=${event.code} reason=${event.reason}`);
	}
	else {
		LogToGeneralLog('[close] Connection died', true);
		show_custom_alert("Connection to server closed. Please reload the site.", "alarm");
	}
};

socket.onerror = function (error) {
	LogToGeneralLog(`[error] ${error.message}`, true);
	show_custom_alert(error.message, "alarm")
};
function UpdateUiTargets(json_dict)
{
	summary = PlayerAttackSummary.CreateFromDict(json_dict);
	UpdateUiPlayerAttackedMonsterWithWeapon(summary);
}
async function FetchActiveMonster() {
	const obj = {
		request_key: "FetchActiveMonster",
		request_data: JSON.stringify({
			player_id: _player.player_id
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

async function AttackMonster() {
	if (_player != undefined) {
		if (_player.IsAlive()) {
			const obj = {
				request_key: "PlayerAttackMonsterWithEquippedWeapon",
				request_data: JSON.stringify({
					player_id: _player.player_id
				})
			};
			const json_obj = JSON.stringify(obj);
			socket.send(json_obj);
		}
		else {
			BlinkDiv("player_hp")
			LogToGeneralLog("Can't attack when you are knocked out", true);
		}
	}
	else {
		LogToGeneralLog("You are not logged in", true)
	}
}

async function SendSignInRequest(pUserName, pPassword) {
	const obj = {
		request_key: "SignIn",
		request_data: JSON.stringify({
			userName: pUserName,
			password: pPassword
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

async function EquipWeapon(weapon_id_to_equip) {
	const obj = {
		request_key: "EquipWeapon",
		request_data: JSON.stringify({
			player_id: _player.player_id,
            weapon_id: weapon_id_to_equip
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

async function SellWeapon(weapon_id_to_equip) {
	const obj = {
		request_key: "SellWeapon",
		request_data: JSON.stringify({
			player_id: _player.player_id,
            weapon_id: weapon_id_to_equip
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

async function FetchMostRecentMessages(messages_to_fetch=10) {
    if (messages_to_fetch <= 100) {
        const obj = {
            request_key: "FetchMostRecentMessages",
            request_data: JSON.stringify({
                messages_to_fetch: messages_to_fetch
            })
        };
        const json_obj = JSON.stringify(obj);
        socket.send(json_obj);
    }
}

async function FetchMonsterVotesTotals() {
	const obj = {
		request_key: "FetchMonsterVotesTotals",
		request_data: null
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}


function playerAttemptedToBuyAPlayerClass(params_dict) {
	const sucess = params_dict["sucess"];
	const message = params_dict["message"];
	if (sucess) {
		const updated_player = params_dict["updated_player"];
		const player = Player.CreateFromDict(updated_player);
		UpdateUiPlayerStats(player);
		show_custom_alert(message, "congratulate");
	}
	else {
		show_custom_alert(message, "alarm");
	}
}

async function SendChangePlayerClassRequest(player_id, player_class_id, preffered_body_type) {
	const obj = {
		request_key: "ChangePlayerClass",
		request_data: JSON.stringify({
			player_id: parseInt(player_id),
			player_class_id: parseInt(player_class_id),
			preffered_body_type: preffered_body_type,
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

// TEST WebSocket button
async function LoginTestUser() {
	await SendSignInRequest("Demo", "A");
}

// TEST WebSocket button2
async function LoginTestUser2() {
	await SendSignInRequest("Test", "B");
}
