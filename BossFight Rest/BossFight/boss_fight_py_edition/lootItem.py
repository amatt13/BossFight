import math


class LootItem:
    def __init__(self, loot_id: int, loot_name: str, loot_drop_chance: float, cost: int):
        self.loot_id: int = loot_id
        self.loot_name: str = loot_name
        self.loot_drop_chance: float = loot_drop_chance
        self.cost: int = cost

    def set_name(self, name: str):
        self.loot_name = name

    def get_name(self) -> str:
        return self.loot_name

    def get_sell_price(self) -> int:
        return math.ceil(self.cost / 4)
