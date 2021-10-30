class DamageTrackerEntry:
    def __init__(self, player_id, player_name, duration, damage):
        self._player_id = player_id
        self._player_name = player_name
        self._duration = duration
        self._damage = damage

    def to_dict(self) -> dict:
        return {"player_id": self._player_id, "player_name": self._player_name, "duration": self._duration, "damage": self._damage}

    def subtract_turn(self, n: int = 1):
        self._duration -= n

    def get_duration(self):
        return self._duration

    def get_player_id(self):
        return self._player_id

    def get_player_name(self):
        return self._player_name

    def get_damage(self):
        return self._damage
