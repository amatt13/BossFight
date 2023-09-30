using BossFight.Models;

namespace BossFight.Models
{
    public class Smite : Ability
    {
        public Smite()
        : base("Smite", "Deals bonus damage to undead monsters", 6)
        { }
    }

    public class Execute : Ability
    {
        private const float _monster_health_percentage = 20.0f;

        public Execute()
        : base("Execute", $"Deals massive damage to monsters that are near death (hp below { _monster_health_percentage }%)", -1)
        { }
    }

    public class SackOnHead : Ability
    {
        private const int _blind_duration = 2;

        public SackOnHead()
        : base("Sack on head", $"Blinds the target by putting a sack on their head (duration { _blind_duration })", 8)
        { }
    }

    public class DoubleStrike : Ability
    {
        public DoubleStrike()
        : base("Double strike", "Hit the monster twice", 5)
        { }
    }

    public class PoisonedBait : Ability
    {
        public PoisonedBait()
        : base("Poisoned bait", "Lure in the monster with some poisoned food", 4)
        { }
    }

    public class Hex : Ability
    {
        private const int _hex_duration = 3;

        public Hex()
        : base("Hex", $"Curse the monster, lowering its attack damage (duration {_hex_duration})", 3)
        { }
    }

    public class FractureSkin : Ability
    {
        private const int _fractureSkin_skin_duration = 5;

        public FractureSkin()
        : base("Fracture skin", $"Curse the monster, making its skin crack and fracture to deal continuous damage (duration {_fractureSkin_skin_duration})", 6)
        { }
    }

    public class Ignite : Ability
    {
        private const int _ignite_duration = 2;

        public Ignite()
        : base("Ignite", $"Ignite the monster, setting it on fire and dealing some additional damage (duration {_ignite_duration})", 5)
        { }
    }

    public class EnchantWeapon : Ability
    {
        public EnchantWeapon()
        : base("Enchant Weapon", "Enchant the target's weapon, making it deal bonus magical damage (cast on self if no target is designated)", 4)
        { }
    }

    public class Shout : Ability
    {
        public Shout()
        : base("Shout", "Increase all players' attack and lower monster's", 8)
        { }
    }

    public class Frenzy : Ability
    {
        public Frenzy()
        : base("Frenzy", "Swing you weapon in blood raged frenzy and apply a Frenzy Stack! Frenzy deals bonus damage depending on Frenzy Stacks", 10)
        { }
    }

    public class Sacrifice : Ability
    {
        public Sacrifice()
        : base("Sacrifice", "Sanctify weapon with your blood to deal bonus holy damage. Also deals additional damage to undead monsters", 8)
        { }
    }

    public class GreaterHeal : Ability
    {
        public GreaterHeal()
        : base("Greater Heal", "A potent healing spell", 15)
        { }
    }

    public class TurnWeaponToSilver : Ability
    {
        public TurnWeaponToSilver()
        : base("Turn weapon to silver", "Turn your weapon's material into silver to deliver a devastating blow to the exotic monster", 6)
        { }
    }

    public class OverSizedBearTrap : Ability
    {
        public OverSizedBearTrap()
        : base("Over sized bear trap", "Place a trap for the monster to step into, stunning it and making it bleed in the process", 12)
        { }
    }

    public class BigGameTrophy : Ability
    {
        public BigGameTrophy()
        : base("Big game trophy", "Claim the monster's head as your trophy. Making it easier to crit, and killing it gives you bonus gold.", 14)
        { }
    }
}
