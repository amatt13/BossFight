using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BossFight.CustemExceptions;

namespace BossFight.Models
{
    public class Ability
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ITarget Caster { get; set; }
        public ITarget Target { get; set; }
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
