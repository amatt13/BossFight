document.onkeydown = function(evt) {
    evt = evt || window.event;
    if ("key" in evt && (evt.key === "Escape" || evt.key === "Esc")) {
		if (playerClassMenu.style.display == 'block' && playerClassMenuBackground.style.display == 'block') {
			CloseMenu();
		}else if (shopDialog.style.display == 'block' && dialogBackground.style.display == 'block') {
			CloseShop();
		}
    }
};
