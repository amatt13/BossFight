let openShopBottun = document.getElementById('openShopButton');
let shopDialog = document.getElementById('shopDialog');
let dialogBackground = document.getElementById('dialogBackground');
let closeButton = document.getElementById('closeShopButton');


openShopBottun.addEventListener('click', function onOpen() {
    dialogBackground.style.display = 'block';
    shopDialog.style.display = 'block';
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
