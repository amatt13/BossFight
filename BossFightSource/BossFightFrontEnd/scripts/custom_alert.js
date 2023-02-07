// Trigger action when the contexmenu is about to be shown
function show_custom_alert(text_message, type) {
    let icon = "default.image";
    switch(type) {
        case "alert": 
            icon = "alert.image";
            break;
        case "congrateulate": 
            icon = "congrateulate.image";
            break;
    }
    document.getElementById("custom-alert-text-label").innerHTML = text_message;

    $(".custom-alert").finish().toggle(100);
}

// hide if something else is clicked
$(document).bind("mousedown", function (e) {
    if (!$(e.target).parents(".custom-alert").length > 0) {
        $(".custom-alert").hide(100);
    }
});
