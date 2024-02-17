let _other_players_window_player_ids_list = []


function makeDraggable (element) {
    // Make an element draggable (or if it has a .window-top class, drag based on the .window-top element)
    let currentPosX = 0, currentPosY = 0, previousPosX = 0, previousPosY = 0;

		// If there is a window-top classed element, attach to that element instead of full window
    if (element.querySelector('.window-top')) {
        // If present, the window-top element is where you move the parent element from
        element.querySelector('.window-top').onmousedown = dragMouseDown;
    }
    else {
        // Otherwise, move the element itself
        element.onmousedown = dragMouseDown;
    }

    function dragMouseDown (e) {
        // Prevent any default action on this element (you can remove if you need this element to perform its default action)
        e.preventDefault();
        // Get the mouse cursor position and set the initial previous positions to begin
        previousPosX = e.clientX;
        previousPosY = e.clientY;
        // When the mouse is let go, call the closing event
        document.onmouseup = closeDragElement;
        // call a function whenever the cursor moves
        document.onmousemove = elementDrag;
    }

    function elementDrag (e) {
        // Prevent any default action on this element (you can remove if you need this element to perform its default action)
        e.preventDefault();
        // Calculate the new cursor position by using the previous x and y positions of the mouse
        currentPosX = previousPosX - e.clientX;
        currentPosY = previousPosY - e.clientY;
        // Replace the previous positions with the new x and y positions of the mouse
        previousPosX = e.clientX;
        previousPosY = e.clientY;

        // Set the element's new position
        let new_y = element.offsetTop - currentPosY;
        let new_x = element.offsetLeft - currentPosX;

        xy = _reduceXYTobeWithInBounds(element, new_x, new_y);
        new_x = xy["x"];
        new_y = xy["y"];

        element.style.top = new_y + 'px';
        element.style.left = new_x + 'px';
    }

    function closeDragElement () {
        // Stop moving when mouse button is released and release events
        document.onmouseup = null;
        document.onmousemove = null;
        localStorage.setItem("otherPlayersWindowY", element.style.top.match(/\d+/)[0])
        localStorage.setItem("otherPlayersWindowX", element.style.left.match(/\d+/)[0])
    }
}

// Make myWindow and myWindow2 draggable in different ways...

// myWindow will only be able to be moved via the top bar (.window-top element). The main element does nothing on mouse down.
makeDraggable(document.querySelector('#otherPlayersWindow'));

// myWindow2 will be able to moved by grabbing the entire element
// makeDraggable(document.querySelector('#myWindow2'));


function _reduceXYTobeWithInBounds(element, x, y) {
    const max_y = Math.max(document.documentElement.clientHeight, window.innerHeight) - element.clientHeight;
    const min_y = 0;
    const max_x = Math.max(document.documentElement.clientWidth, window.innerWidth) - element.clientWidth;
    const min_x = 0;

    if (x > max_x) {
        x = max_x;
    }
    else if (x < min_x) {
        x = min_x;
    }

    if (y > max_y) {
        y = max_y;
    }
    else if (y < min_y) {
        y = min_y;
    }

    return{"x": x, "y": y};
}

function populateOtherPlayersWindow(other_players_list) {
    let other_players_window = document.querySelector('#otherPlayersWindow');
    if (other_players_window.hidden) {
        other_players_window.hidden = false;
        y = localStorage.getItem("otherPlayersWindowY");
        x = localStorage.getItem("otherPlayersWindowX");
        if (x != null && y != null) {
            const xy = _reduceXYTobeWithInBounds(other_players_window, x, y);
            x = xy["x"];
            y = xy["y"];
            other_players_window.style.top = y + "px";
            other_players_window.style.left = x + "px";
        }
    }

    let window_content = other_players_window.getElementsByClassName("window-content")[0];
    let new_html = window_content.innerHTML;
    let player_ids_list = [..._other_players_window_player_ids_list]
    other_players_list.forEach(player => {
        if (!player_ids_list.includes(player.player_id)) {
            player_ids_list.push(player.player_id)
            new_html += _createOtherPlayerCard(player);
        }
    });
    _other_players_window_player_ids_list = player_ids_list

    window_content.innerHTML = new_html;
    let table_rows = document.getElementsByClassName("other-players-window-table");
    for (row of table_rows) {
        row.onmousedown = _changeCurrentPlayerTarget;
    }
}

function getCurrentPlayerTarget() {
    let result = null;

    let selected = document.querySelector('#otherPlayersWindow').querySelector(".window-content").querySelector(".other-players-window-row-selected");
    if (selected != undefined)
        result = selected.playerid;

    return result;
}

function _changeCurrentPlayerTarget(player_target) {
    clearCurrentPlayerTarget();
    player_target.currentTarget.classList.add("other-players-window-row-selected");
}

function clearCurrentPlayerTarget() {
    let selected = document.querySelector('#otherPlayersWindow').querySelector(".window-content").querySelector(".other-players-window-row-selected");
    if (selected != undefined)
        selected.classList.remove("other-players-window-row-selected");
}

// This is the PlayerClass "cards"
function _createOtherPlayerCard(player) {
	const sprite_source = getPlayerClassSprite(player.player_class_name, player.preffered_body_type.name).src;
	const card_html = `<table class="other-players-window-table" id="otherPlayersWindowTable">
		<tr class="other-players-window-row">
			<td class="other-players-window-table-image">
				<img id="player_class_menu_player_class${ player.player_class_name }_sprite" src="${ sprite_source }" width="75" height="75">
			</td>
            <td class="other-players-window-table-row-info" playerid="${ player.player_id }">
                <p>${ player.name } - ${ player.current_hp }/${ player.max_hp } HP - ${ player.current_mana }/${ player.max_mana } mana</p>
            </td>
		</tr>
	</table>`

	return card_html;
}
