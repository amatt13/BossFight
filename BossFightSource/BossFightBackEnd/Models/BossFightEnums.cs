namespace BossFight.BossFightEnums
{
    public enum MonsterType
    {
        HUMANOID,
        UNDEAD,
        BEAST,
        DRAGON,
        DEMON,
        MAGIC_CREATURE
    }

    public enum MonsterTierVoteChoice
    {
        DECREASE_DIFFICULTY = -1,
        UNCHANGED = 0,
        INCREASE_DIFFICULTY = 1
    }

    public enum PlayerClassEnum
    {
        CLERIC = 1,
        HIGHWAYMAN = 2,
        RANGER = 3,
        HEXER = 4,
        MAGE = 5,
        BARBARIAN = 6,
        MONSTER_HUNTER = 7,
        PALADIN = 8
    }
}
