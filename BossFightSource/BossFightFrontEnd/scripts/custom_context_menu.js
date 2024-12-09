// Reference to the player inventory
let player_inventory = document.getElementById('player_inventory');

// Function to hide all context menus
function hideAllContextMenus() {
    document.querySelectorAll('.context-menu').forEach(menu => {
        menu.classList.remove('visible');
    });
}

// Add a click event listener to the document to hide context menus when clicking outside
document.addEventListener("click", (e) => {
    // Check if the clicked element is not inside a context menu
    if (!e.target.closest(".context-menu")) {
        hideAllContextMenus();
    }
});

// Add a contextmenu event to the player inventory
player_inventory.addEventListener("contextmenu", function (event) {
    if (event.target.tagName === "OPTION") {
        // Select the right-clicked option
        event.target.selected = true;

        // Set custom menu data attributes
        document.getElementById("custom_menu_equip").dataset.equip_id = event.target.dataset.weapon_id;
        document.getElementById("custom_menu_sell").dataset.sell_id = event.target.dataset.weapon_id;

        // Prevent the default browser context menu
        event.preventDefault();

        // Show and position the custom context menu
        const contextMenu = document.querySelector(".inventory-context-menu");
        contextMenu.style.top = `${event.pageY}px`;
        contextMenu.style.left = `${event.pageX}px`;
        contextMenu.classList.add('visible');
    }
});

// Add click listeners for inventory context menu actions
document.querySelectorAll(".inventory-context-menu li").forEach(menuItem => {
    menuItem.addEventListener("click", function () {
        const action = this.dataset.action;

        switch (action) {
            case "equip":
                const equip_id = parseInt(document.getElementById("custom_menu_equip").dataset.equip_id);
                EquipWeapon(equip_id);
                break;
            case "sell":
                const sell_id = parseInt(document.getElementById("custom_menu_sell").dataset.sell_id);
                SellWeapon(sell_id);
                break;
        }

        // Hide the context menu after the action
        hideAllContextMenus();
    });
});

// Add click listeners for ability context menu actions
document.querySelectorAll(".ability-context-menu li").forEach(menuItem => {
    menuItem.addEventListener("click", function () {
        const action = this.dataset.action;

        switch (action) {
            case "cast on self":
                const selfAbilityName = document.getElementById("custom_menu_cast_on_self").dataset.ability_name;
                playerCast(selfAbilityName, _player.player_id);
                break;
            case "cast on player target":
                const targetAbilityName = document.getElementById("custom_menu_cast_on_player_target").dataset.ability_name;
                const targetPlayerId = document.getElementById("custom_menu_cast_on_player_target").dataset.target_player_id;
                playerCast(targetAbilityName, targetPlayerId);
                break;
        }

        // Hide the context menu after the action
        hideAllContextMenus();
    });
});

// Function to show the ability context menu
function showContextMenu(event) {
    const abilityName = event.target.dataset.ability_name;

    // Set data attributes for the custom menu options
    const customMenuTarget = document.getElementById("custom_menu_cast_on_player_target");
    customMenuTarget.dataset.target_player_id = getCurrentPlayerTarget();
    customMenuTarget.dataset.ability_name = abilityName;

    const customMenuSelf = document.getElementById("custom_menu_cast_on_self");
    customMenuSelf.dataset.ability_name = abilityName;

    // Show and position the ability context menu
    const contextMenu = document.querySelector(".ability-context-menu");
    contextMenu.style.top = `${event.pageY}px`;
    contextMenu.style.left = `${event.pageX}px`;
    contextMenu.classList.add('visible');
}
