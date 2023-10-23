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

        public static AbilityResult CastAbility(string pAbilityName, ITarget pCaster, int pTargetId, ILogger<SocketMessageHandler> pLogger)
        {
            Ability ability = null;
            var result = new AbilityResult();
            try
            {
                ability = (Ability)Activator.CreateInstance(AbilityDictionary[pAbilityName]);
            }
            catch (KeyNotFoundException e)
            {
                pLogger.LogError(e.Message);
                result.Error = $"An internal server error occured when trying to cast {pAbilityName}";
            }

            if (ability != null)
            {
                ITarget target;
                if (ability.OnlyTargetMonster)
                {
                    if (RequestValidator.MonsterInstanceExists(pTargetId, out MonsterInstance monsterTarget, out string error))
                    {
                        target = monsterTarget;
                    }
                    else
                    {
                        result.Error = error;
                        return result;
                    }
                }
                else
                {
                    target = pCaster;
                }

                result = ability.UseAbility(pCaster, target, result);
            }

            return result;
        }
    }
}
