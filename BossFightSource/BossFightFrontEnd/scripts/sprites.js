function file_exists(path)
{
    let result = false; 

    var http = new XMLHttpRequest();
    http.open('HEAD', "findfiles/" + path.split("/").at(-1), false);
    try {
        http.send();
        result = http.status == 200;
    } catch (error) {
        result = false;
    }

    return result;
}


function getPlayerClassSprite(class_name, preffered_body_type) {
    let img = new Image();
    
    player_class_name_suffix = ""
    switch (preffered_body_type) {
        case "masculine":            
        case 2:
            player_class_name_suffix = "_m"
            break;
        case "feminine":
        case 1:
            player_class_name_suffix = "_f"
            break;
        default:
            break;
    }

    path = `./images/sprites/player_classes/${class_name}${player_class_name_suffix}.png`;
    const player_class_with_preffered_body_type_exists = file_exists(path);
    if (player_class_with_preffered_body_type_exists) {
        img.src = path;
    } else {
        path = `./images/sprites/player_classes/${class_name}_m.png`;
        img.src = path;
    }

    return img;
}
