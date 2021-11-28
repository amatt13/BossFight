let sendChatMessageButton = document.getElementById("sendChatMessageButton");
let chatMessageContentInput = document.getElementById("chatMessageContentInput");

sendChatMessageButton.addEventListener("click", function() {
    if (_player != undefined) {
        let chatText = chatMessageContentInput.value.trim().substr(0, 300);
        if (chatText.length > 0) {
            chatText = _player.name + ": " + chatText;
            chatMessageContentInput.value = "";
            const obj = {
                //TODO validate that the user is who they pretend to be (server-side)
                request_key: "SendChatMessage",
                request_data: JSON.stringify({
                    "message": chatText
                })
            };
            const json_obj = JSON.stringify(obj);
            socket.send(json_obj);
        }
    }
    else {
        LogToTextLog("You are not logged in", true)
    }
})
