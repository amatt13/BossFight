const openPlayerClassShopBottun = document.getElementById('openPlayerClassShopButton');
let playerClassShopDialog = document.getElementById('playerClassShopDialog');
let dialogBackground = document.getElementById('dialogBackground');
let closeButton = document.getElementById('closeShopButton');


openPlayerClassShopBottun.addEventListener('click', function onOpen() {
    const obj = {
		request_key: "GetPlayerClassShopForPlayer",
		request_data: JSON.stringify({
			player_id: _player.player_id
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
    //TODO add "ativity spinner" and delete below code. Only show the dialog when we have recived an answer in "UpdateUiShop()"
    dialogBackground.style.display = 'block';
    playerClassShopDialog.style.display = 'block';
    document.getElementById("shop_gold_amount_label").innerHTML = `Gold: ${ _player.gold }`;
});

closeButton.addEventListener('click', function onOpen() {
    CloseShop();
});

dialogBackground.addEventListener('click', function onOpen() {
    CloseShop();
});

function BuyPlayerClass(player_class_id) {
    const obj = {
		request_key: "BuyPlayerClass",
		request_data: JSON.stringify({
            player_id: _player.player_id,
            player_class_id: player_class_id
        })
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
}

function CloseShop() {
    playerClassShopDialog.style.display = 'none';
    dialogBackground.style.display = 'none';
}
