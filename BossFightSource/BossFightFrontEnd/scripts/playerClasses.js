const openUnlockedClassesButton = document.getElementById('openUnlockedClassesButton');
let playerClassMenu = document.getElementById('playerClassMenu');
let playerClassMenuBackground = document.getElementById('dialogBackground');
let closeplayerClassMenuButton = document.getElementById('closeplayerClassMenuButton');
let playerClassMenuMasculineFeminineSliderCheckbox = document.getElementById("playerClassMenuMasculineFeminineSliderCheckbox");

// Request the list of classes
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

let __player_player_classes_instances = new Array();
// Build the menu and make it visible
function showPlayerClassesMenu(player_player_classes) {
	__player_player_classes_instances = new Array();
    player_player_classes.forEach(player_class_dict => {
        const player_player_class = PlayerPlayerClass.CreateFromDict(player_class_dict);
        __player_player_classes_instances.push(player_player_class);
    });

    PopulatePlayerClassList(__player_player_classes_instances);
	const active_player_class = __player_player_classes_instances.find(pc => pc.active == true)
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

// This is the PlayerClass "cards"
function CreatePlayerclassTitleCardForPlayerClassMenu(playerclass) {
	const sprite_source = getPlayerClassSprite(playerclass.name).src;
	const card_html = `<table class="playerClassSelectorTable" id="playerClassSelectorTable">
		<tr class="playerClassSelectorRow">
			<td >
				<img id="player_class_menu_player_class${ playerclass.name }_sprite" src="${ sprite_source }" width="75" height="75" style="object-fit: fill;">
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

	// Add click event that changes what class whould be displayed in the details pane
	document.getElementById("unlockedPlayerClasses").innerHTML = player_player_class_html;
	const containers = document.getElementsByClassName("playerClassSelectorContainer")
	for (let con of containers) {
		con.addEventListener("click", () => {
			const player_class_id = con.attributes["playerClassId"].value;
			const clicked_player_class = __player_player_classes_instances.find(pci => pci.player_class.player_class_id == player_class_id);
			changePlayerClassSelectorCurrentlySelectedRow(player_class_id);
			setLeftPaneToSelectedClass(clicked_player_class);
		});
	}
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
	title.innerHTML = __current_player_class.name;
	sprite = document.getElementById("playerClassSelectorLeftPaneSprite");

	let sprite_source = new Image();
	if (playerClassMenuMasculineFeminineSliderCheckbox.checked) {
		sprite_source = getPlayerClassSprite(__current_player_class.name, "feminine");
	}
	else {
		sprite_source = getPlayerClassSprite(__current_player_class.name, "masculine");
	}
	sprite.src = sprite_source.src;
}
