// Trigger action when the contexmenu is about to be shown
function show_custom_alert(text_message, type) {
    let icon = "default.png";
    switch(type) {
        case "alarm": 
            icon = "alarm.png";
            break;
        case "congratulate": 
            icon = "congratulate.png";
            break;
    }
    document.getElementById("custom-alert-text-icon").src = "./images/ui_icons/alert_menu/" + icon;

    text_message = text_message.replace("\n", "<br>");
    document.getElementById("custom-alert-text-label").innerHTML = text_message;

    $(".custom-alert").finish().toggle(100);
}

// hide if something else is clicked
$(document).bind("mousedown", function (e) {
    if (!$(e.target).parents(".custom-alert").length > 0) {
        $(".custom-alert").hide(100);
    }
});
