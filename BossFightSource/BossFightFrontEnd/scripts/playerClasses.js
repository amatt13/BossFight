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

function showPlayerClassesMenu(player_classes) {
    PopulatePlayerClassList(player_classes)
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


function CreatePlayerclassTitleCardForPlayerClassMenu(playerclass, row) {
	const card_html = `<div style="border: solid; border-color: var(--border-colour); margin-left: 5px">
		<table class="playerClassSelectorTable">
			<tr class="playerClassSelectorRow">
				<td >
					<img id="player_class_menu_player_class${ playerclass.name }_sprite" src="./images/sprites/player_classes/${ playerclass.name }.png" width="75" height="75" style="object-fit: fill;">
				</td>
				<td class="playerClassSelectorRowPlayerClassQuickDescription">
					<label >${ playerclass.name }<br>${ playerclass.description }</label>
				</td>
			</tr>
		</table>
	</div>`

	return card_html;
}

function PopulatePlayerClassList(player_classes) {
	let player_classes_instances = new Array();
    player_classes.forEach(player_class_dict => {
        const player_player_class = PlayerPlayerClass.CreateFromDict(player_class_dict)
        player_classes_instances.push(player_player_class);
    });

	let player_player_class_html = "";

	player_classes_instances.forEach((ppc, i) => {
		const player_player_class_count = i + 2;
		_playerclasses_list.push(ppc.player_Class);
		const card = CreatePlayerclassTitleCardForPlayerClassMenu(ppc.player_Class, player_player_class_count);
		const class_row = `<div id="PlayerClassContainer" style="grid-column: 2; grid-row: ${ player_player_class_count };">
		${ card }
	</div>`;
    player_player_class_html += class_row;
	});

	document.getElementById("unlockedPlayerClasses").innerHTML = player_player_class_html;
}
