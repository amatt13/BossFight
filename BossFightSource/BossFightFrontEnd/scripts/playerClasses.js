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

playerClassMenuMasculineFeminineSliderCheckbox.addEventListener("change", function onChange() {
	showPlayerClassesMenuHelper();
});

let __player_player_classes_instances = new Array();
// Build the menu and make it visible
function showPlayerClassesMenu(player_player_classes) {
	playerClassMenuMasculineFeminineSliderCheckbox.checked = _player.preffered_body_type.Name == "feminine";
	__player_player_classes_instances = new Array();
    player_player_classes.forEach(player_class_dict => {
        const player_player_class = PlayerPlayerClass.CreateFromDict(player_class_dict);
        __player_player_classes_instances.push(player_player_class);
    });

    showPlayerClassesMenuHelper();
}

function showPlayerClassesMenuHelper() {
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

setToCurrentClassPlayerClassMenuButton.addEventListener('click', function onOpen() {
    CloseMenu();
	let player_class_id = -1;

	const player_class_selector_row_selected_row = document.getElementsByClassName("playerClassSelectorRowSelectedRow");
	let inspect_element = player_class_selector_row_selected_row[0];
	while (inspect_element.parentElement != null) {
		if (inspect_element.classList.contains("playerClassSelectorContainer")) {
			player_class_id = inspect_element.attributes["playerclassid"].value;
			break;
		} else {
			inspect_element = inspect_element.parentElement;
		}
	}

	const preffered_body_type = playerClassMenuMasculineFeminineSliderCheckbox.checked ? "feminine" : "masculine";
	SendChangePlayerClassRequest(_player.player_id, player_class_id, preffered_body_type);
});

function CloseMenu() {
    playerClassMenu.style.display = 'none';
    playerClassMenuBackground.style.display = 'none';
}

// This is the PlayerClass "cards"
function CreatePlayerclassTitleCardForPlayerClassMenu(playerclass) {
	const body_type = playerClassMenuMasculineFeminineSliderCheckbox.checked ? "feminine" : "masculine";
	const sprite_source = getPlayerClassSprite(playerclass.name, body_type).src;
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

function _setLeftPaneBasics(player_player_class) {
	player_class = player_player_class.player_class;

	let player_class_menu_required_classes = document.getElementById("playerClassMenuRequiredClasses");
	let requirements_list = [];
	player_class.player_class_requirement_list.forEach(pcr => {
		requirements_list.push(`${ pcr.required_player_class_name } level ${ pcr.level_requirement }`)
	});
	if (requirements_list.length == 0)
		requirements_list.push("None")
	player_class_menu_required_classes.textContent = "Class requirements: " + requirements_list.join("; ");

	let player_class_menu_gold_cost = document.getElementById("playerClassMenuGoldCost");
	player_class_menu_gold_cost.textContent = "Shop gold cost: " + player_class.purchase_price;

	let player_class_menu_description = document.getElementById("playerClassMenuDescription");
	player_class_menu_description.textContent = player_class.description;

	// basics
	let player_class_menu_attack_power_bonus = document.getElementById("playerClassMenuAttackPowerBonus");
	player_class_menu_attack_power_bonus.textContent = "Attack power bonus: +" + player_class.attack_power_bonus;

	let player_class_menu_current_level = document.getElementById("playerClassMenuCurrentLevel");
	player_class_menu_current_level.textContent = "Current level: " + player_player_class.level;

	let player_class_menu_max_health_points = document.getElementById("playerClassMenuMaxHealthPoints");
	player_class_menu_max_health_points.textContent = "Max health points: " + player_player_class.max_hp;

	let player_class_menu_health_points_gained_per_level = document.getElementById("playerClassMenuHealthPointsGainedPerLevel");
	player_class_menu_health_points_gained_per_level.textContent = "Health points per level: " + player_class.hp_scale;

	let player_class_menu_max_mana_åoints = document.getElementById("playerClassMenuMaxManaPoints");
	player_class_menu_max_mana_åoints.textContent = "Max mana points: " + player_player_class.max_mana;

	let player_class_menu_mana_points_gained_per_level = document.getElementById("playerClassMenuManaPointsGainedPerLevel");
	player_class_menu_mana_points_gained_per_level.textContent = "Mana points per level: " + player_class.mana_scale;

	let player_class_menu_crit_chance = document.getElementById("playerClassMenuCritChance");
	player_class_menu_crit_chance.textContent = "Base critical chance: " + player_class.crit_chance;

	let player_class_menu_abilities_list = document.getElementById("PlayerClassMenuAbilitiesList");
	player_class_menu_abilities_list.innerHTML = "";
	player_class.abilities.forEach(ability => {
		const ability_html = `<button id="preview_ability_${ability.name}" type="button" class="preview-btn-ability" disabled="true">
	<img src="./images/ui_icons/abilities/${ability.image_source}.png" class="max-size-100-percent">
</button>`
		player_class_menu_abilities_list.innerHTML += ability_html;
	});

}

__current_player_class = null;
function setLeftPaneToSelectedClass(player_player_class) {
	__current_player_class = player_player_class.player_class;
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

	_setLeftPaneBasics(player_player_class);
}
