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
        $(".custom-menu").finish().toggle(100).
        
        // In the right position (the mouse)
        css({
            top: event.pageY + "px",
            left: event.pageX + "px"
        });
    }
});


// If the document is clicked somewhere
$(document).bind("mousedown", function (e) {
    
    // If the clicked element is not the menu
    if (!$(e.target).parents(".custom-menu").length > 0) {
        
        // Hide it
        $(".custom-menu").hide(100);
    }
});


// If the menu element is clicked
$(".custom-menu li").click(function(){
    
    // This is the triggered action name
    switch($(this).attr("data-action")) {
        
        // A case for each action. Your actions here
        case "equip": 
            var equip_id = document.getElementById("custom_menu_equip").dataset.equip_id;
            EquipWeapon(equip_id);
            break;
        case "sell": 
            var sell_id = document.getElementById("custom_menu_sell").dataset.sell_id;
            SellWeapon(sell_id);
            break;
    }
  
    // Hide it AFTER the action was triggered
    $(".custom-menu").hide(100);
  });
