using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace BossFight.Models
{
    public class Ability
    {
        public string Name { get; private set; }

        public string AbilityCastKey { get {return GetType().Name; }}

        public string Description { get; set; }

        [JsonIgnore]
        public ITarget Caster { get; set; }

        [JsonIgnore]
        public ITarget Target { get; set; }

        [JsonIgnore]
        public StringBuilder UseAbilityText { get; set; }

        public bool OnlyTargetMonster { get; set; }

        public int ManaCost { get; set; }

        public bool AffectsAllPlayers { get; set; }

        public Ability(string pName, string pDescription, int pManaCost)
        {
            Name = pName;
            Description = pDescription;
            Caster = null;
            Target = null;
            UseAbilityText = new StringBuilder();
            OnlyTargetMonster = false;
            ManaCost = pManaCost;
            AffectsAllPlayers = false;
        }

        public override string ToString()
        {
            var onlyTargetMonsterString = "";
            if (OnlyTargetMonster)
                onlyTargetMonsterString = "*";

            return $"{ ManaCost } mana - /**{ Name }**{ onlyTargetMonsterString } -> { Description }";
        }

        public virtual string UseAbility(ITarget pCaster, ITarget pTarget)
        {
            string result;
            var errors = new StringBuilder();
            Caster = pCaster;
            Target = pTarget;

            if (CanCastAbility(errors)) {
                TargetEffect(pTarget);
                SubtractManaCostFromCaster();
                result = UseAbilityText.ToString();
            }
            else
            {
                result = $"Could not use ability {Name}";
                if (errors.Length >= 1)
                {
                    result += Environment.NewLine;
                    result += errors.ToString();
                }
            }

            return result.TrimEnd(new char[] { '\r', '\n' });
        }

        public virtual bool CanCastAbility(StringBuilder pError)
        {
            var canCastAbility = true;
            if (ManaCost >= Caster.Mana)
            {
                pError.AppendLine("You do not have enough mana");
                canCastAbility = false;
            }
            else if (Caster.IsDead())
            {
                pError.AppendLine("You are knocked out");
                canCastAbility = false;
            }

            if (Caster is Player player)
            {
                var playerPlayerClass = player.PlayerPlayerClass;
                var playerClass = playerPlayerClass.PlayerClass.RecalculateUnlockedAbilities(playerPlayerClass.Level).Any(ability => ability.Name == this.Name);  // Just check for the name... should be good enough
            }

            return canCastAbility;
        }

        // The effect that will be executed on the target
        public virtual void TargetEffect(ITarget pTarget)
        { }

        public virtual void AffectsAllPlayersEffect(List<Player> allPlayers)
        { }

        public void SubtractManaCostFromCaster()
        {
            Caster.Mana -= ManaCost;
        }
    }
}
