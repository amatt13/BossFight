using System;
using System.Collections.Generic;
using BossFight.Models;
using Microsoft.Extensions.Logging;

namespace BossFight.Controllers
{
    public static class AbilityController
    {
        private readonly static Dictionary<string, Ability> AbilityDictionary = new()
        {
            {nameof(Heal), new Heal()},
            {nameof(DivineShield), new DivineShield()},
        };

        public static string CastAbility(string pAbilityName, ITarget pCaster, ITarget pTarget, ILogger<SocketMessageHandler> pLogger)
        {
            string result;
            try
            {
                Ability ability = AbilityDictionary[pAbilityName];
                result = ability.UseAbility(pCaster, pTarget);
            }
            catch (KeyNotFoundException e)
            {
                #pragma warning disable CA2254
                pLogger.LogError(e.Message);
                result = $"An internal server error occured when trying to cast {pAbilityName} on {pTarget.Name}";
            }

            return result;
        }
    }
}
