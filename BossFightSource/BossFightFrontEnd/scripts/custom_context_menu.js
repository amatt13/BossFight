// If the document is clicked somewhere
$(document).bind("mousedown", function (e) {

    // If the clicked element is not the menu
    if (!$(e.target).parents(".context-menu").length > 0) {

        // Hide it
        $(".context-menu").hide(100);
    }
});


$("#player_inventory").bind("contextmenu", function (event) {

    if (event.target.tagName == "OPTION") {
        // select the right clicked option
        event.target.selected = true;
        document.getElementById("custom_menu_equip").dataset.equip_id = event.target.dataset.weapon_id;
        document.getElementById("custom_menu_sell").dataset.sell_id = event.target.dataset.weapon_id;

        // Avoid the real one
        event.preventDefault();

        // Show contextmenu
        $(".inventory-context-menu").finish().toggle(100).css({
            // In the right position (the mouse)
            top: event.pageY + "px",
            left: event.pageX + "px"
        });
    }
});

document.querySelectorAll(".inventory-context-menu li").forEach(inventory_element => {
    inventory_element.addEventListener("click", function () {
        // This is the triggered action name
        switch ($(this).attr("data-action")) {

            // A case for each action. Your actions here
            case "equip":
                const equip_id = document.getElementById("custom_menu_equip").dataset.equip_id;
                const equip_id_int = parseInt(equip_id)
                EquipWeapon(equip_id_int);
                break;
            case "sell":
                const sell_id = document.getElementById("custom_menu_sell").dataset.sell_id;
                const sell_id_int = parseInt(sell_id)
                SellWeapon(sell_id_int);
                break;
        }
        // Hide it AFTER the action was triggered
        $(".inventory-context-menu").hide(100);
    });
});

document.querySelectorAll(".ability-context-menu li").forEach(ability_element => {
    ability_element.addEventListener("click", function () {
        switch ($(this).attr("data-action")) {

            // A case for each action. Your actions here
            case "cast on self":
                const custom_menu_cast_on_self = document.getElementById("custom_menu_cast_on_self");
                const self_ability_name = custom_menu_cast_on_self.dataset.ability_name;
                player_cast(self_ability_name, _player.player_id);
                break;
            case "cast on player target":
                const custom_menu_cast_on_player_target = document.getElementById("custom_menu_cast_on_player_target");
                const target_ability_name = custom_menu_cast_on_player_target.dataset.ability_name;
                const target_id = custom_menu_cast_on_player_target.dataset.target_player_id;
                player_cast(target_ability_name, target_id);
                break;
        }
        $(".ability-context-menu").hide(100);
    });
});

function showContextMenu(event) {
    const button_ability_name = event.target.dataset.ability_name;

    let custom_menu_cast_on_player_target = document.getElementById("custom_menu_cast_on_player_target");
    custom_menu_cast_on_player_target.dataset.target_player_id = getCurrentPlayerTarget();
    custom_menu_cast_on_player_target.dataset.ability_name = button_ability_name;

    let custom_menu_cast_on_self = document.getElementById("custom_menu_cast_on_self");
    custom_menu_cast_on_self.dataset.ability_name = button_ability_name;

    $(".ability-context-menu").finish().toggle(100).css({
        top: event.pageY + "px",
        left: event.pageX + "px"
    });
}
