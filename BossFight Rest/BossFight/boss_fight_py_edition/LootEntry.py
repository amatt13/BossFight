class LootEntry:
    def __init__(self, player_id: str, damage_dealt_by_player: int, gold_earned: int):
        from source.games.boss_fight import player
        self.player_id = player_id
        self.damage_dealt_by_player = damage_dealt_by_player
        self.relative_damage_dealt_by_player = 0.0  # this player's damage divided by total damage dealt to monster
        self.gold_earned = gold_earned
        self.player_object: player.Player = player.Player("NA", "")

    def set_player(self, p):
        self.player_object = p  # player
