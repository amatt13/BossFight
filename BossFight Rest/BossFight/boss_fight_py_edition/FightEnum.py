import enum


class FightEnum(enum.Enum):
    def __str__(self):
        # Use '_' for spaces -> fx 'MAGIC_CREATURE' = 'Magic Creature'
        return str(self.name).replace('_', ' ').capitalize()


class MonsterType(int, FightEnum):
    HUMANOID = enum.auto()
    UNDEAD = enum.auto()
    BEAST = enum.auto()
    DRAGON = enum.auto()
    DEMON = enum.auto()
    MAGIC_CREATURE = enum.auto()


class WeaponType(int, FightEnum):
    POLEARM = enum.auto()
    THROWN = enum.auto()
    IMPROVISED = enum.auto()
    MACE = enum.auto()
    DAGGER = enum.auto()
    BOW = enum.auto()
    SWORD = enum.auto()
    AXE = enum.auto()
    STAFF = enum.auto()
