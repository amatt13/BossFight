const sendChatMessageButton = document.getElementById("sendChatMessageButton");
const chatMessageContentInput = document.getElementById("chatMessageContentInput");


function PopulateChatLogWithMultipleMessages(message_dictionaries) {
    message_dictionaries.sort((a,b) => Date.parse(a["Timestamp"]) - Date.parse(b["Timestamp"])).forEach(message_dict => {
        ReceiveChatMessage(message_dict, false)
    });
}

chatMessageContentInput.addEventListener("keypress", function(event) {
  if (event.key === "Enter") {
    sendChatMessageButton.click();
  }
})

sendChatMessageButton.addEventListener("click", function() {
    if (_player != undefined) {
        let chatText = chatMessageContentInput.value.trim().substr(0, 300);
        if (chatText.length > 0) {
            chatMessageContentInput.value = "";
            const obj = {
                //TODO validate that the user is who they pretend to be (server-side)
                request_key: "SendChatMessage",
                request_data: JSON.stringify({
                    "message": chatText,
                    "player_id": _player.player_id
                })
            };
            const json_obj = JSON.stringify(obj);
            socket.send(json_obj);
        }
    }
    else {
        LogToGeneralLog("You are not logged in", true)
    }
})

function OpenLog(evt, logName) {
    let i, x, tablinks;

    x = document.getElementsByClassName("log");
    for (i = 0; i < x.length; i++) {
      x[i].style.display = "none";
    }

    tablinks = document.getElementsByClassName("log-tab");
    for (i = 0; i < tablinks.length; i++) {
      tablinks[i].className = tablinks[i].className.replace(" border-red", "");
    }

    document.getElementById(logName).style.display = "block";
    evt.currentTarget.firstElementChild.className += " border-red";
  }

function ReceiveChatMessage(chat_message_dict, blink = true) {
    chat_message = ChatMessage.CreateFromDict(chat_message_dict);

    const combined_message_text = `${chat_message.player_name}: ${chat_message.message_content}`
    LogToChatLog(combined_message_text, blink)
}

function LogToGeneralLog(pText, blink = false) {
	document.getElementById("text_log").value += pText + '\n'
	if (blink) {
		BlinkDiv("text_log_hover", 'red');
	}
}

function LogToCombatLog(pText, blink = false, colur = "yellow") {
	document.getElementById("combat_log").value += pText + '\n'
	if (blink) {
		BlinkDiv("combat_log_hover", colur);
	}
}

function LogToChatLog(pText, blink = false) {
	document.getElementById("chat_log").value += pText + "\n"
	if (blink) {
		BlinkDiv("chat_log_hover");
	}
}
