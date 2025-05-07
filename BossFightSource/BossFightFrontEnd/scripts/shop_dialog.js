const openPlayerClassShopBottum = document.getElementById('openPlayerClassShopButton');
let playerClassShopDialog = document.getElementById('playerClassShopDialog');
let playerClassShopCloseButton = document.getElementById('playerClassShopCloseShopButton');

const openItemShopBottum = document.getElementById('openItemShopButton');
let itemShopDialog = document.getElementById('itemShopDialog');
let itemShopCloseButton = document.getElementById('itemShopCloseShopButton');

let dialogBackground = document.getElementById('dialogBackground');


openPlayerClassShopBottum.addEventListener('click', function onOpen() {
    const obj = {
		request_key: "GetPlayerClassShopForPlayer",
		request_data: JSON.stringify({
			player_id: _player.player_id
		})
	};
	const json_obj = JSON.stringify(obj);
	socket.send(json_obj);
    //TODO add "ativity spinner" and delete below code. Only show the dialog when we have recived an answer in "UpdateUiShop()"
    _setGoldAmountLabels();
    dialogBackground.style.display = 'block';
    playerClassShopDialog.style.display = 'block';
});

openItemShopBottum.addEventListener('click', function onOpen() {
    _setGoldAmountLabels();
    dialogBackground.style.display = 'block';
    itemShopDialog.style.display = 'block';
});

function _setGoldAmountLabels()
{
    let gold_labels = document.getElementsByClassName("shop-gold-amount-display");
    for (let item of gold_labels)
    {
        item.innerHTML = `Gold: ${ _player.gold }`;
    }
}

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

dialogBackground.addEventListener('click', function onOpen() {
    ClosePlayerClassShop();
    CloseItemShop();
});

playerClassShopCloseButton.addEventListener('click', function onOpen() {
    ClosePlayerClassShop();
});

function ClosePlayerClassShop() {
    playerClassShopDialog.style.display = 'none';
    dialogBackground.style.display = 'none';
}

itemShopCloseButton.addEventListener('click', function onOpen() {
    CloseItemShop();
});

function CloseItemShop() {
    itemShopDialog.style.display = 'none';
    dialogBackground.style.display = 'none';
}
