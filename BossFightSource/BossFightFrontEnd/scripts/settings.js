const playerSettingsButton = document.getElementById("playerSettings");

playerSettingsButton.addEventListener("click", function onOpen(){
    // show settings dialog
    let dialog = document.getElementById("playerSettingsDialog");
    dialog.style.display = "block";

    let dialogBackground = document.getElementById('dialogBackground');
    dialogBackground.style.display = 'block';

    let _closeButton = document.getElementById('closeSettingsMenuButton');

    _closeButton.addEventListener('click', function onOpen() {
        closeSettingsDialog();
    });

    dialogBackground.addEventListener('click', function onOpen() {
        closeSettingsDialog();
    });

    function closeSettingsDialog() {
        dialog.style.display = 'none';
        dialogBackground.style.display = 'none';
    }
});

