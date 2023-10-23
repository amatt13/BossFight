using System;

namespace BossFight.Models
{
    public class AbilityResult
    {
        public string Error;
        public string AbilityResultText;
        public PlayerAttackSummary PlayerAttackSummary;
        public bool ReloadMonster;
        public bool CastSuccess;

        public AbilityResult() {}
    }
}
