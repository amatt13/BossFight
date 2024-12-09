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
    // Set the text, image, and visibiliy of the alert
    let alert_icon = document.getElementById("custom-alert-text-icon");
    alert_icon.src = "./images/ui_icons/alert_menu/" + icon

    let alert_label = document.getElementById("custom-alert-text-label");
    alert_label.textContent = "";
    const lines = text_message.split("\n");
    lines.forEach((line, index) => {
        if (index > 0) {
            alert_label.appendChild(document.createElement("br"));
        }
        alert_label.appendChild(document.createTextNode(line));
    });

    let alert_menu = document.getElementById("custom-alert-menu");
    alert_menu.style.display = "inline";
    function hideAlertDialog(event) {
        if (!alert_menu.contains(event.target)) {
            alert_menu.style.display = "none";
            document.removeEventListener("click", hideAlertDialog, true);
        }
    }
    document.addEventListener("click", hideAlertDialog, true);
}
