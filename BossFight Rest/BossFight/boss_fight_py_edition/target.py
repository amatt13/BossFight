class Target:
    def __init__(self, hp: int = 1, name: str = "No name", level: int = 1):
        self.hp: int = hp
        self.name: str = name
        self.level: int = level

    def get_max_hp(self) -> int:
        raise NotImplementedError("get_max_hp() not implemented for class")

    def is_dead(self) -> bool:
        return self.hp <= 0

    def is_alive(self) -> bool:
        return not self.is_dead()

    def is_at_full_health(self) -> bool:
        return self.hp >= self.get_max_hp()

    def possessive_name(self) -> str:
        if self.name[-1:] == 's':
            return self.name + "'"
        else:
            return self.name + "'s"
