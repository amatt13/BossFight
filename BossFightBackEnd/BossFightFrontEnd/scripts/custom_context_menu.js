// Trigger action when the contexmenu is about to be shown
$("#player_inventory").bind("contextmenu", function (event) {
    
    if (event.target.tagName == "OPTION")
    {
        // select the right clicked option
        event.target.selected = true;
        document.getElementById("custom_menu_equip").dataset.equip_id = event.target.dataset.weapon_id;
        document.getElementById("custom_menu_sell").dataset.sell_id = event.target.dataset.weapon_id;

        // Avoid the real one
        event.preventDefault();
        
        // Show contextmenu
        $(".context-menu").finish().toggle(100).css({
            // In the right position (the mouse)
            top: event.pageY + "px", 
            left: event.pageX + "px"
        });
    }
});


// If the document is clicked somewhere
$(document).bind("mousedown", function (e) {
    
    // If the clicked element is not the menu
    if (!$(e.target).parents(".context-menu").length > 0) {
        
        // Hide it
        $(".context-menu").hide(100);
    }
});


// If the menu element is clicked
$(".context-menu li").click(function(){
    
    // This is the triggered action name
    switch($(this).attr("data-action")) {
        
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
    $(".context-menu").hide(100);
  });
