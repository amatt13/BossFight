namespace BossFight.Models.Ability
{
    public class Ability
    {
        public Ability(object name = str, object description = str, object mana_cost = @int, object magic_word = str)
        {
            this.name = name;
            this.description = description;
            this.caster = null;
            this.use_ability_text = "";
            this.only_target_monster = false;
            this.mana_cost = mana_cost;
            this.magic_word = magic_word.lower();
            this.affects_all_players = false;
            this.affects_all_players_str = "";
        }

        public override string ToString()
        {
            var only_target_monster = "";
            if (this.only_target_monster)
            {
                only_target_monster = "*";
            }
            return "{self.mana_cost} mana - **{self.magic_word}**/**{self.name}**{only_target_monster} -> {self.description}";
        }

        public virtual string use_ability(object caster, object target_target = target.Target, object dont_use_caster_effect = false)
        {
            this.use_ability_text = "";
            this.caster = caster;
            if (this.only_target_monster && !(target_target is target.Target))
            {
                throw TypeError("Can only target monsters");
            }
            if (!dont_use_caster_effect)
            {
                this.caster_effect();
            }
            if (target_target != null)
            {
                await this.target_effect(target_target);
            }
            this.subtract_mana_cost_from_caster();
            this.add_mana_text();
            return this.affects_all_players_str + this.use_ability_text;
        }

        // The effect that will be executed on the caster
        public virtual void caster_effect()
        {
        }

        // The effect that will be executed on the target
        public virtual void target_effect(object target_target = target.Target)
        {
        }

        public virtual void affects_all_players_effect(object all_players = list[target.Target])
        {
        }

        public void subtract_mana_cost_from_caster()
        {
            if (this.caster.mana < this.mana_cost)
            {
                throw source.games.boss_fight.statics.WTFException("caster mana: {self.caster.mana} mana cost: {self.mana_cost}");
            }
            this.caster.mana -= this.mana_cost;
        }

        public void add_mana_text()
        {
            if (this.use_ability_text.Count > 0 && this.use_ability_text[-1] != "\n")
            {
                this.use_ability_text += "\n";
            }
            this.use_ability_text += "**Mana:** You have { self.caster.mana } mana left";
        }

        public string bold_name_with_colon()
        {
            return $"**{Name}:**";
        }
    }
}
