using System;
using BossFight.BossFightEnums;
using BossFight.CustemExceptions;

namespace BossFight.Models
{
    public static class PlayerClassFactory
    {
        public static PlayerClass CreatePlayerClass(PlayerClassEnum pPlayerClassEnum, int? pPlayerLevel = null)
        {
            PlayerClass playerClass = pPlayerClassEnum switch
            {
                PlayerClassEnum.CLERIC => pPlayerLevel.HasValue ? new Cleric(pPlayerLevel.Value) : new Cleric(),
                PlayerClassEnum.HIGHWAYMAN => pPlayerLevel.HasValue ? new Highwayman(pPlayerLevel.Value) : new Highwayman(),
                PlayerClassEnum.RANGER => pPlayerLevel.HasValue ? new Ranger(pPlayerLevel.Value) : new Ranger(),
                PlayerClassEnum.HEXER => pPlayerLevel.HasValue ? new Hexer(pPlayerLevel.Value) : new Hexer(),
                PlayerClassEnum.MAGE => pPlayerLevel.HasValue ? new Mage(pPlayerLevel.Value) : new Mage(),
                PlayerClassEnum.BARBARIAN => pPlayerLevel.HasValue ? new Barbarian(pPlayerLevel.Value) : new Barbarian(),
                PlayerClassEnum.MONSTER_HUNTER => pPlayerLevel.HasValue ? new MonsterHunter(pPlayerLevel.Value) : new MonsterHunter(),
                PlayerClassEnum.PALADIN => pPlayerLevel.HasValue ? new Paladin(pPlayerLevel.Value) : new Paladin(),
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
