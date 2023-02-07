const openShopBottun = document.getElementById('openShopButton');
let shopDialog = document.getElementById('shopDialog');
let dialogBackground = document.getElementById('dialogBackground');
let closeButton = document.getElementById('closeShopButton');


openShopBottun.addEventListener('click', function onOpen() {
    const obj = {
		request_key: "GetShopForPlayer",
		request_data: JSON.stringify({
			player_id: _player.player_id
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
    //TODO add "ativity spinner" and delete below code. Only show the dialog when we have recived an answer in "UpdateUiShop()"
    dialogBackground.style.display = 'block';
    shopDialog.style.display = 'block';
    document.getElementById("shop_gold_amount_label").innerHTML = `Gold: ${ _player.gold }`;
});

closeButton.addEventListener('click', function onOpen() {
    CloseShop();
});

dialogBackground.addEventListener('click', function onOpen() {
    CloseShop();
});

function CloseShop() {
    shopDialog.style.display = 'none';
    dialogBackground.style.display = 'none';
}
