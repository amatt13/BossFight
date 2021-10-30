class AttackMessage:
    def __init__(self, player, monster):
        self.player = player
        self.monster = monster
        self.player_crit = False
        self.weapon_attack_message = ""
        self.monster_retaliate_message = ""
        self.player_extra_damage_from_buffs = False
        self.player_xp_earned = 0
        self.monster_affected_by_dots = ""
        self.ability_extra = ""

    def __str__(self):
        p_crit = ""
        w_atc_message = ""
        p_ex = ""
        p_buffs = ""
        m_retl_message = ""
        m_aff_by_dots = ""
        ability_extra = ""
        if self.player_crit:
            p_crit = "**CRITICAL HIT**\n"
        if len(self.weapon_attack_message) > 0:
            w_atc_message = f"**Attack:** {self.weapon_attack_message}\n"
        if self.player_xp_earned > 0:
            p_ex = f"**XP:** You received {self.player_xp_earned} xp\n"
        if self.player_extra_damage_from_buffs:
            p_buffs = f"**Buffs:** You dealt extra bonus damage because of your buffs\n"
        if len(self.monster_retaliate_message) > 0:
            m_retl_message = f"**Monster attack:** {self.monster_retaliate_message}\n"
        if len(self.monster_affected_by_dots) > 0:
            m_aff_by_dots = f"**Monster dots:** {self.monster_affected_by_dots}\n"
        if len(self.ability_extra):
            ability_extra = f"{self.ability_extra}\n"
        return f"{p_crit}{w_atc_message}{p_ex}{m_retl_message}{p_buffs}{m_aff_by_dots}{ability_extra}"
