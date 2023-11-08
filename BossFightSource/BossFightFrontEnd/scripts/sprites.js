function buildPlayerSpriteName(class_name, preffered_body_type, include_file_ending) {
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

    result = `${class_name}${player_class_name_suffix}`.toLocaleLowerCase();
    if (include_file_ending) {
        result += ".png"
    }

    return result;
}

function getPlayerClassSprite(class_name, preffered_body_type) {
    let img = new Image();

    class_name = class_name.toLocaleLowerCase();
    full_class_name_with_file_ending = buildPlayerSpriteName(class_name, preffered_body_type, true);
    path = `./images/sprites/player_classes/${full_class_name_with_file_ending}`;
    img.src = path;

    return img;
}
