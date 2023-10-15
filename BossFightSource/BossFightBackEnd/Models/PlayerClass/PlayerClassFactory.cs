using BossFight.BossFightEnums;
using BossFight.CustemExceptions;

namespace BossFight.Models
{
    public static class PlayerClassFactory
    {
        public static PlayerClass CreatePlayerClass(PlayerClassEnum pPlayerClassEnum)
        {
            PlayerClass playerClass = pPlayerClassEnum switch
            {
                PlayerClassEnum.CLERIC => new Cleric(),
                PlayerClassEnum.HIGHWAYMAN => new Highwayman(),
                PlayerClassEnum.RANGER => new Ranger(),
                PlayerClassEnum.HEXER => new Hexer(),
                PlayerClassEnum.MAGE => new Mage(),
                PlayerClassEnum.BARBARIAN => new Barbarian(),
                PlayerClassEnum.MONSTER_HUNTER => new MonsterHunter(),
                PlayerClassEnum.PALADIN => new Paladin(),
                _ => throw new InvalidPlayerClassException($"Could not find a valid class for value {(int)pPlayerClassEnum}"),
            };

            return playerClass;
        }

        public static PlayerClass CreatePlayerClass(PlayerPlayerClass pPlayerPlayerClass)
        {
            var playerClassEnum = (PlayerClassEnum)pPlayerPlayerClass.PlayerClassId.Value;
            return CreatePlayerClass(playerClassEnum);
        }

        public static PlayerClass CreatePlayerClass(int pPlayerClassId)
        {
            var playerClassEnum = (PlayerClassEnum)pPlayerClassId;
            return CreatePlayerClass(playerClassEnum);
        }
    }
}
