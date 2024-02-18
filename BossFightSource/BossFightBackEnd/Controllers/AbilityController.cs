using System;
using System.Collections.Generic;
using BossFight.Models;
using Microsoft.Extensions.Logging;

namespace BossFight.Controllers
{
    #pragma warning disable CA2254

    public static class AbilityController
    {
        private readonly static Dictionary<string, Type> AbilityDictionary = new()
        {
            {nameof(Heal), typeof(Heal)},
            {nameof(DivineShield), typeof(DivineShield)},
            {nameof(GreaterHeal), typeof(GreaterHeal)},
            {nameof(Smite), typeof(Smite)},
            //{nameof(XXX), typeof(XXX)},
        };

        public static ITarget FindTargetForAbility(int pTargetId, ITarget pCaster, Ability pAbility, ref string pError)
        {
            ITarget target = null;

            if (pAbility.OnlyTargetMonster)
            {
                if (RequestValidator.MonsterInstanceExists(pTargetId, out MonsterInstance monsterTarget, out string error))
                {
                    target = monsterTarget;
                }
                else
                {
                    pError = error + "\nMonster was not found";
                }
            }
            else
            {
                if (pCaster.Id == pTargetId)
                {
                    target = pCaster;
                }
                else
                {
                    if (RequestValidator.PlayerExists(pTargetId, out Player playerTarget, out string error))
                    {
                        target = playerTarget;
                    }
                    else
                    {
                        pError = error + "\nPlayer was not found";;
                    }
                }
            }

            return target;
        }

        public static Ability CreateAbility(string pAbilityName, ref string pError)
        {
            Ability ability = null;
            try
            {
                ability = (Ability)Activator.CreateInstance(AbilityDictionary[pAbilityName]);
            }
            catch (KeyNotFoundException e)
            {
                pError = e.Message;
            }

            return ability;
        }
    }
}
