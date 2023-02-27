const openUnlockedClassesButton = document.getElementById('openUnlockedClassesButton');
let playerClassMenu = document.getElementById('playerClassMenu');
let playerClassMenuBackground = document.getElementById('dialogBackground');
let closeplayerClassMenuButton = document.getElementById('closeplayerClassMenuButton');

openUnlockedClassesButton.addEventListener('click', function onOpen() {
    const obj = {
		request_key: "GetUnlockedClassesForPlayer",
		request_data: JSON.stringify({
			player_id: _player.player_id
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
});

function showPlayerClassesMenu(player_player_classes) {
	let player_player_classes_instances = new Array();
    player_player_classes.forEach(player_class_dict => {
        const player_player_class = PlayerPlayerClass.CreateFromDict(player_class_dict);
        player_player_classes_instances.push(player_player_class);
    });

    PopulatePlayerClassList(player_player_classes_instances);
	const active_player_class = player_player_classes_instances.find(pc => pc.active == true)
	if (active_player_class != undefined) {
		changePlayerClassSelectorCurrentlySelectedRow(active_player_class.player_class.player_class_id);
		setLeftPaneToSelectedClass(active_player_class);
	}
    playerClassMenuBackground.style.display = 'block';
    playerClassMenu.style.display = 'block';
}

closeplayerClassMenuButton.addEventListener('click', function onOpen() {
    CloseMenu();
});

playerClassMenuBackground.addEventListener('click', function onOpen() {
    CloseMenu();
});

function CloseMenu() {
    playerClassMenu.style.display = 'none';
    playerClassMenuBackground.style.display = 'none';
}


function CreatePlayerclassTitleCardForPlayerClassMenu(playerclass) {
	const card_html = `<table class="playerClassSelectorTable" id="playerClassSelectorTable">
		<tr class="playerClassSelectorRow">
			<td >
				<img id="player_class_menu_player_class${ playerclass.name }_sprite" src="./images/sprites/player_classes/${ playerclass.name }.png" width="75" height="75" style="object-fit: fill;">
			</td>
			<td class="playerClassSelectorRowPlayerClassQuickDescription">
				<label >${ playerclass.name }<br>${ playerclass.description }</label>
			</td>
		</tr>
	</table>`

	return card_html;
}

function PopulatePlayerClassList(player_player_classes) {
	let player_player_class_html = "";

	player_player_classes.forEach((ppc, i) => {
		const player_player_class_count = i + 2;
		_playerclasses_list.push(ppc.player_class);
		const card = CreatePlayerclassTitleCardForPlayerClassMenu(ppc.player_class);
		const class_row = `<div class="playerClassSelectorContainer" playerClassId="${ppc.player_class.player_class_id}" style="grid-column: 2; grid-row: ${ player_player_class_count };">
		${ card }
	</div>`;
    player_player_class_html += class_row;
	});

	document.getElementById("unlockedPlayerClasses").innerHTML = player_player_class_html;
}

// mark a new table row as the "active" row
function changePlayerClassSelectorCurrentlySelectedRow(player_class_id) {
	const all_player_class_selector_containers = document.getElementsByClassName("playerClassSelectorContainer")
	for (var i = 0; i != all_player_class_selector_containers.length; i++) {
		var player_class_container = all_player_class_selector_containers[i];
		if ("playerclassid" in player_class_container.attributes) {
			var row = player_class_container.getElementsByTagName("table")[0].getElementsByTagName("tbody")[0].getElementsByClassName("playerClassSelectorRow")[0];
			row.classList.remove("playerClassSelectorRowSelectedRow");
			if (player_class_container.attributes["playerClassId"].value == player_class_id) {
				row.classList.add("playerClassSelectorRowSelectedRow");
			}
		}
	};
}

__current_player_class = null;
function setLeftPaneToSelectedClass(current_player_player_class) {
	__current_player_class = current_player_player_class.player_class;
	title = document.getElementById("playerClassSelectorLeftPaneTitle");
	title.text = __current_player_class.name;
	sprite = document.getElementById("playerClassSelectorLeftPaneSprite");
	sprite.src = `./images/sprites/player_classes/${ __current_player_class.name }.png`
}
