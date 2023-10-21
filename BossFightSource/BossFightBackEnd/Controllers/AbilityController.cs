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

        public static string CastAbility(string pAbilityName, ITarget pCaster, ITarget pTarget, ILogger<SocketMessageHandler> pLogger)
        {
            string result;
            try
            {
                Ability ability = (Ability)Activator.CreateInstance(AbilityDictionary[pAbilityName]);
                result = ability.UseAbility(pCaster, pTarget);
            }
            catch (KeyNotFoundException e)
            {
                pLogger.LogError(e.Message);
                result = $"An internal server error occured when trying to cast {pAbilityName} on {pTarget.Name}";
            }

            return result;
        }
    }
}
