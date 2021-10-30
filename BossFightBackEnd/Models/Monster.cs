using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using BossFight.BossFightEnums;
using BossFight.CustemExceptions;
using BossFight.Models.Loot;

namespace BossFight.Models
{
    // public static object recreateMonster(object monsterName = str)
    // {
    //     foreach (var Tup1 in inspect.getmembers(sys.modules[@Name]))
    //     {
    //         var name = Tup1.Item1;
    //         var obj = Tup1.Item2;
    //         if (inspect.isclass(obj) && issubclass(obj, Monster))
    //         {
    //             try
    //             {
    //                 var monsterInstance = obj();
    //                 if (monsterInstance.name == monsterName)
    //                 {
    //                     return monsterInstance;
    //                 }
    //             }
    //             catch (TypeError)
    //             {
    //             }
    //         }
    //     }
    // }

    public class MonsterKilledEventArgs : EventArgs
    {
        public int KillerId { get; set; }
        public Monster DeadMonster { get; set; }
    }

    public class Monster : Target
    {
        public const string ERRORIMAGEURL = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTZ5TWeaqjdWuvgco4oq5N50bWNGwE-eJDGpg&usqp=CAU";
        public const int TIERDIVIDER = 5;
        public const float MONSTERCRITMODIFIER = 1.5f;
        private static Random _random = new Random();

        public Dictionary<int, int> DamageTracker { get; set; }  // key is PlayerId, value is totaled damge from player
        public int MaxHp { get; set; }
        public string ImageUrl { get; set; }
        public List<MonsterType> MonsterTypeList { get; set; }
        public bool BossMonster { get; set; }
        public int BlindDuration { get; set; }
        public int StunDuration { get; set; }
        public int LowerAttackDuration { get; set; }
        public float LowerAttackPercentage { get; set; }
        public int EasierToCritDuration { get; set; }
        public int EasierToCritPercentage { get; set; }
        public Dictionary<int, DamageTrackerEntry> DamageOverTimeTracker { get; set; }  // key is player_id
        public Dictionary<int, int> FrenzyStackTracker { get; set; }  // key is player_id, value is frenzy stack lvl/size
        public event EventHandler<MonsterKilledEventArgs>  MonsterDied;

        public Monster(string pName, string pImageUrl, Dictionary<int, int> pDamageTracker = null, List<MonsterType> pMonsterTypes = null, bool pBossMonster = false)
        {
            DamageTracker = pDamageTracker ?? new Dictionary<int, int>();
            MaxHp = 1;
            ImageUrl = pImageUrl;
            MonsterTypeList = pMonsterTypes ?? new List<MonsterType> { MonsterType.HUMANOID };
            BossMonster = pBossMonster;
            BlindDuration = 0;
            StunDuration = 0;
            LowerAttackDuration = 0;
            LowerAttackPercentage = 0.0f;
            EasierToCritDuration = 0;
            EasierToCritPercentage = 0;
            DamageOverTimeTracker = new Dictionary<int, DamageTrackerEntry>();
            FrenzyStackTracker = new Dictionary<int, int>();
        }

        public Monster(string pName, string pImageUrl, Dictionary<int, int> pDamageTracker, MonsterType? pMonsterType = MonsterType.HUMANOID, bool pBossMonster = false)
            : this(pName, pImageUrl, pDamageTracker, new List<MonsterType> { pMonsterType ?? MonsterType.HUMANOID }, pBossMonster)
        { }

        public Monster(string pName, string pImageUrl, MonsterType pMonsterType = MonsterType.HUMANOID)
            : this(pName, pImageUrl, pMonsterTypes: new List<MonsterType> { pMonsterType })
        { }

        public Monster(string pName, string pImageUrl, bool pBossMonster, MonsterType pMonsterType = MonsterType.HUMANOID)
            : this(pName, pImageUrl, pMonsterTypes: new List<MonsterType> { pMonsterType }, pBossMonster: pBossMonster)
        { }

         public Monster(string pName, string pImageUrl)
            : this(pName, pImageUrl, pMonsterTypes: new List<MonsterType> { MonsterType.HUMANOID })
        { }

        // public static object fromDict(object cls, object monsterDict = dict)
        // {
        //     var m = recreateMonster(monsterDict["name"]);
        //     m.hp = monsterDict["hp"];
        //     m.maxHp = monsterDict["maxHp"];
        //     m.level = monsterDict["level"];
        //     m.damageTracker = monsterDict["damageTracker"];
        //     m.blindDuration = Convert.ToInt32(monsterDict["blindDuration"]);
        //     m.stunDuration = Convert.ToInt32(monsterDict["stunDuration"]);
        //     m.lowerAttackDuration = monsterDict["lowerAttackDuration"];
        //     m.lowerAttackPercentage = monsterDict["lowerAttackPercentage"];
        //     m.easierToCritDuration = monsterDict["easierToCritDuration"];
        //     m.easierToCritPercentage = monsterDict["easierToCritPercentage"];
        //     m.frenzyStackTracker = monsterDict["frenzyStackTracker"];
        //     var damageTrackerEntryDictList = monsterDict["damageOverTimeTracker"];
        //     foreach (var entry in damageTrackerEntryDictList)
        //     {
        //         var pId = entry["playerId"];
        //         m.damageOverTimeTracker[pId] = DamageTrackerEntry(pId, entry["playerName"], entry["duration"], entry["damage"]);
        //     }
        //     return m;
        // }

        // public virtual object toDict()
        // {
        //     var monsterDict = new Dictionary<object, object>
        //     {
        //     };
        //     var dmgOverTimeList = (from d in DamageOverTimeTracker.values()
        //                            select d.toDict()).ToList();
        //     monsterDict["damageOverTimeTracker"] = dmgOverTimeList;
        //     monsterDict["blindDuration"] = BlindDuration;
        //     monsterDict["stunDuration"] = BlindDuration;
        //     monsterDict["damageTracker"] = DamageTracker;
        //     monsterDict["hp"] = Hp;
        //     monsterDict["level"] = Level;
        //     monsterDict["lowerAttackDuration"] = LowerAttackDuration;
        //     monsterDict["lowerAttackPercentage"] = LowerAttackPercentage;
        //     monsterDict["easierToCritDuration"] = EasierToCritDuration;
        //     monsterDict["easierToCritPercentage"] = EasierToCritPercentage;
        //     monsterDict["maxHp"] = MaxHp;
        //     monsterDict["name"] = Name;
        //     monsterDict["frenzyStackTracker"] = FrenzyStackTracker;
        //     return monsterDict;
        // }

        public override string ToString()
        {
            var isDead = "";
            var boss = "";
            var dots = "";
            var monsterType = "";
            var hasLoot = "";
            if (MonsterTypeList.Any(mt => mt != MonsterType.HUMANOID))
                monsterType = $" ({ MonsterTypesStr() })";

            if (GetItemDrops().Any())
                hasLoot = "⭐";

            if (IsDead())
                isDead = "\n**Monster is dead**";
                
            if (BossMonster)
                boss = "**---BOSS---**\n";
                
            if (DamageOverTimeTracker.Keys.Any())
                dots = $"\nDot damage next turn: { DamageOverTimeTracker.Sum(dot => dot.Value.GetDamage()) }";
                
            var debuffs = DebuffsString();
            return $"{ boss }{ Name }{ hasLoot }{ monsterType } : Level { Level }\nHP: { HP }/{ MaxHp }{ debuffs }{ dots }{ isDead }";
        }

        public bool HasMonsterType(List<MonsterType> pMonsterTypeList)
        {
            return pMonsterTypeList.Any(mt => HasMonsterType(mt));
        }

        public bool HasMonsterType(MonsterType monsterType)
        {
            return MonsterTypeList.Contains(monsterType);
        }

        public string MonsterTypesStr()
        {
            var numberOfTypes = MonsterTypeList.Count;
            var typesStr = "";
            if (numberOfTypes == 1)
            {
                typesStr = MonsterTypeList[0].ToString();
            }
            else if (numberOfTypes == 2)
            {
                typesStr = $"{ MonsterTypeList[0] } and { MonsterTypeList[1] }";
            }
            else if (numberOfTypes > 2)
            {
                typesStr = String.Join(", ", (MonsterTypeList.Take(MonsterTypeList.Count - 1)));
                typesStr += $", and { MonsterTypeList.Last() }";
            }
            return typesStr;
        }

        public string DebuffsString()
        {
            var debuffs = new List<object>();
            if (BlindDuration > 0)
                debuffs.Add($"blinded { BlindDuration }");
                
            if (LowerAttackDuration > 0)
                debuffs.Add($"lowered attack by { LowerAttackPercentage * 100 }% for { LowerAttackDuration } attacks");
                
            if (StunDuration > 0)
                debuffs.Add($"stunned { StunDuration }");
                
            if (EasierToCritDuration > 0)
                debuffs.Add($"easier to crit { EasierToCritPercentage }% for { EasierToCritDuration } attacks");
                
            var result = "";
            if (debuffs.Any())
                result += "\n";
                
            return result + String.Join("\n", debuffs);
        }

        public void SetLevelAndHealth(int pLevel)
        {
            Level = pLevel;
            CalcHealth();
        }

        public override int GetMaxHp()
        {
            return MaxHp;
        }

        public void CalcHealth()
        {
            var hp = 10d;
            foreach (var i in Enumerable.Range(0, Level))
            {
                if (BossMonster)
                {
                    hp += hp * 0.22;
                }
                else
                    hp += hp * 0.15;
            }
            hp = Math.Floor(hp);
            if (!BossMonster)
            {
                var variance = (int) Math.Floor(hp / 100 * 15);
                hp += _random.Next(-variance, variance + 1);
            }
            MaxHp = (int)hp;
            HP = (int)hp;
        }

        public void DebuffBlind(int pDuration)
        {
            if (BlindDuration < pDuration)
                throw new MyException("Cant blind monster. A stronger blind is already in effect");
            BlindDuration = pDuration;
        }

        public void DebuffStun(int pDuration)
        {
            if (StunDuration < pDuration)
                throw new MyException("Cant stun monster. A stronger stun is already in effect");
            StunDuration = pDuration;
        }

        public void DebuffLowerDmg(int pDuration, float pLowerAttackPercentage)
        {
            if (LowerAttackDuration < pDuration || LowerAttackPercentage < pLowerAttackPercentage)
                throw new MyException("Cant lower attack of monster. A stronger weaken is already in effect");
            LowerAttackDuration = pDuration;
            LowerAttackPercentage = pLowerAttackPercentage;
        }

        public void DebuffDmgOverTime(int pDuration, int pDamage, Player pPlayer)
        {
            foreach (var trackerPlayerId in DamageOverTimeTracker.Keys)
            {
                if (trackerPlayerId != pPlayer.PlayerId)
                    throw new MyException("Each players is limited to a single damage over time effect");
            }
            DamageOverTimeTracker[pPlayer.PlayerId] = new DamageTrackerEntry(pPlayer.PlayerId, pPlayer.Name, pDuration, pDamage);
        }

        public int DebuffApplyFrenzyStack(int pFrenzyStacksToApply, Player pPlayer)
        {
            if (FrenzyStackTracker.Keys.Contains(pPlayer.PlayerId))
            {
                FrenzyStackTracker[pPlayer.PlayerId] += pFrenzyStacksToApply;
            }
            else
                FrenzyStackTracker[pPlayer.PlayerId] = pFrenzyStacksToApply;
            
            // return current stack size for player
            return FrenzyStackTracker[pPlayer.PlayerId];
        }

        public void DebuffEasierToCrit(int pDuration, int pExtraCritChance)
        {
            if (EasierToCritDuration < pDuration || EasierToCritPercentage < pExtraCritChance)
                throw new MyException("Cant increase critical chance on monster. A stronger effect is already applied");
            EasierToCritDuration = pDuration;
            EasierToCritPercentage = pExtraCritChance;
        }

        public void TakeDamage(int pDamage, int pAttackingPlayerId)
        {
            HP -= pDamage;
            if (IsDead())
            {
                var args = new MonsterKilledEventArgs { DeadMonster = this, KillerId = pAttackingPlayerId };
                OnMonsterKilled(args);
            }
        }

        protected virtual void OnMonsterKilled(MonsterKilledEventArgs e)
        {
            var handler = MonsterDied;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public int ReceiveDamageFromPlayer(Player pAttackingPlayer, Weapon pPlayerWeapon, bool pIsCrit)
        {
            double damage = pPlayerWeapon.AttackPower + pAttackingPlayer.GetAttackBonus();
            if (pIsCrit)
                damage *= 1.5;

            if (damage <= 0.0)
                damage = 1;
                
            // variance
            double variance = (pAttackingPlayer.GetLevel() / 5) - 1;
            if (variance > 0.0)
                damage += _random.Next((int)Math.Floor(-variance), (int)Math.Ceiling(variance) + 1);
                
            damage = Math.Ceiling(damage);
            if (DamageTracker.Keys.Contains(pAttackingPlayer.PlayerId))
            {
                DamageTracker[pAttackingPlayer.PlayerId] += (int)damage;
            }
            else
                DamageTracker[pAttackingPlayer.PlayerId] = (int)damage;
                
            TakeDamage((int)damage, pAttackingPlayer.PlayerId);
            return (int)damage;
        }

        public Tuple<int, List<string>> ReceiveDamageFromDamageOverTimeEffects()
        {
            var extraDamage = 0;
            var playerNames = new List<string>();
            // count up damage and record the players
            foreach (var entry in DamageOverTimeTracker.Values)
            {
                if (IsAlive())
                {
                    TakeDamage(entry.GetDamage(), entry.GetPlayerId());
                    extraDamage += entry.GetDamage();
                    playerNames.Add(entry.GetPlayerName());
                    entry.SubtractTurn();
                }
            }
            // remove debuffs where the duration is 0
            var keysToRemove = (from key in DamageOverTimeTracker.Keys
                                where DamageOverTimeTracker[key].GetDuration() <= 0
                                select key).ToList();
            foreach (var key in keysToRemove)
            {
                DamageOverTimeTracker.Remove(key);
            }
            return Tuple.Create(extraDamage, playerNames);
        }

        public bool MonsterAttackIsCrit()
        {
            var critChance = 3;
            if (BossMonster)
            {
                critChance = 15;
            }
            var roll = _random.Next(1, 101);
            return roll <= critChance;
        }

        public string DealDamageToPlayer(Player pPlayerToAttack)
        {
            var damageText = "";
            var dealDamage = IsAlive();
            if (BlindDuration > 0)
            {
                BlindDuration -= 1;
                if (dealDamage)
                {
                    var affectedByBlind = _random.Next(1, 101) > 33;
                    if (affectedByBlind)
                    {
                        damageText += "Monster is currently blind and cannot hit you. ";
                        dealDamage = false;
                    }
                    else
                        damageText += "Monster resisted blind! ";
                }
            }
            if (StunDuration > 0)
            {
                StunDuration -= 1;
                if (dealDamage)
                {
                    dealDamage = false;
                    damageText += "Monster is currently stunned and cannot hit you. ";
                }
            }
            if (dealDamage)
            {
                // base damage
                double damageDealt = Level / 2;
                // variance
                double variance = (Level / 5) - 1;
                if (variance > 0.0)
                {
                    damageDealt += _random.Next((int)Math.Floor(-variance), (int)Math.Ceiling(variance) + 1);
                }
                // round up to 1
                if (damageDealt <= 0.0)
                {
                    damageDealt = 1;
                }
                // bonus boss damage
                if (BossMonster)
                {
                    damageDealt *= 1.25;
                }
                // crit bonus
                if (MonsterAttackIsCrit())
                {
                    damageDealt *= Monster.MONSTERCRITMODIFIER;
                    damageText += $"{ Name } LANDED A CRITICAL HIT!\n";
                }
                // subtract "lowered attack" debuff
                if (LowerAttackDuration > 0)
                {
                    damageDealt -= damageDealt * LowerAttackPercentage;
                    LowerAttackDuration -= 1;
                }
                // round up
                damageDealt = Math.Ceiling(damageDealt);
                damageText += pPlayerToAttack.ReceiveDamageFromMonster((int)damageDealt, Name);
            }
            var resultText = $"{ damageText }**Player hp:** { pPlayerToAttack.HP }/{ pPlayerToAttack.PlayerClass.MaxHp }";
            return resultText;
        }

        public virtual List<LootItem> GetItemDrops()
        {
            return new List<LootItem>();
        }
    }

    // public static object allMonsters = new List<object>();

    // public static object m = obj();

    // static Module()
    // {
    //     allMonsters.append(m);
    // }

    // public static object s = (from x in allMonsters
    //                           select (x.name == m.name || x.imageUrl == m.imageUrl)).Sum();
}