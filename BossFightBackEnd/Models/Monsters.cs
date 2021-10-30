using System.Collections.Generic;
using BossFight.BossFightEnums;
using BossFight.Models.Loot;

namespace BossFight.Models
{
    public class CuteGoblin : Monster
    {

        public CuteGoblin()
            : base("CuteGoblin", "https://static.vecteezy.com/system/resources/previews/003/009/853/original/cute-goblin-mascot-character-cartoon-icon-illustration-vector.jpg")
        {
        }
    }

    public class Slime : Monster
    {

        public Slime()
            : base("Slime", "https://www.toplessrobot.com/wp-content/uploads/2011/09/Slime.jpg", MonsterType.MAGIC_CREATURE)
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.rustedSword
                };
        }
    }

    public class VillageIdiot : Monster
    {

        public VillageIdiot()
            : base("VillageIdiot", "https://thedromedarytales.files.wordpress.com/2013/03/12europe031092Cropped.jpg?w=460&h=575")
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.apprenticeStaff
                };
        }
    }

    public class Cripple : Monster
    {

        public Cripple()
            : base("Cripple", "https://upload.wikimedia.org/wikipedia/commons/6/64/ACrippledBeggarMovesWithCrutchesAccompaniedByALittlWellcomeV0020357.jpg")
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.sharpStick
                };
        }
    }

    public class GiantCentipede : Monster
    {

        public GiantCentipede()
            : base("GiantCentipede", "https://2e.aonprd.com/Images/Monsters/CentipedeTitanCentipede.png", pMonsterType: MonsterType.BEAST)
        {
        }
    }

    public class DuckSizedHorse : Monster
    {

        public DuckSizedHorse()
            : base("DuckSizedHorse", "https://external-preview.redd.it/Qt8kCZYvjNG7q02v-jV2I10GEQKB298ax3f4u2zVzY.jpg?auto=webp&s=b51bb82d5810d33e6f9a8c09becb8031178eadc8", pMonsterType: MonsterType.BEAST)
        {
        }
    }

    public class DabbingSkeleton : Monster
    {

        public DabbingSkeleton()
            : base("DabbingSkeleton", "https://i.etsystatic.com/13035387/r/il/0b48dc/1310488516/il570xN.1310488516G5is.jpg", pMonsterType: MonsterType.UNDEAD)
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.bone
                };
        }
    }

    public class Murloc : Monster
    {

        public Murloc()
            : base("Murloc", "https://wow.zamimg.com/uploads/screenshots/normal/37172-murloc-lurker.jpg")
        {
        }
    }

    public class DireRat : Monster
    {

        public DireRat()
            : base("DireRat", "https://www.pngkey.com/png/detail/21-210947Dire-rat-pathfinder-dire-rat.png", pMonsterType: MonsterType.BEAST)
        {
        }
    }

    public class BoBo : Monster
    {

        public BoBo()
            : base("BoBo", "https://thumbs.dreamstime.com/b/cartoon-caveman-wooden-club-27864592.jpg")
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.bobosWoodenClub
                };
        }
    }

    public class HellBovine : Monster
    {

        public HellBovine()
            : base("HellBovine", "https://static.wikia.nocookie.net/diablo/images/d/de/Cow.gif", pMonsterTypes: new List<MonsterType> {
                    MonsterType.DEMON,
                    MonsterType.BEAST
            })
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem>();
            //return WeaponList.DIABLO2WEAPONS;
        }
    }

    public class CouncilMember : Monster
    {

        public CouncilMember()
            : base("CouncilMember", "https://static.wikia.nocookie.net/diabloGamepedia/images/9/94/CouncilMember%28DiabloII%29.gif", pMonsterTypes: new List<MonsterType> {
                    MonsterType.DEMON,
                    MonsterType.HUMANOID
            })
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem>();
            //return WeaponList.DIABLO2WEAPONS;
        }
    }

    public class Giant : Monster
    {

        public Giant()
            : base("Giant", "https://static.wikia.nocookie.net/elderscrolls/images/4/4f/Grok.png")
        {
        }
    }

    public class Minotaur : Monster
    {

        public Minotaur()
            : base("Minotaur", "https://cdnb.artstation.com/p/assets/images/images/005/191/851/large/peter-csanyi-minotaur5.jpg?1489173777")
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.battleAxe
                };
        }
    }

    public class MinionOfDestruction : Monster
    {

        public MinionOfDestruction()
            : base("MinionOfDestruction", "https://static.wikia.nocookie.net/diabloGamepedia/images/1/11/MinionOfDestruction%28DiabloII%29.gif", pMonsterType: MonsterType.DEMON)
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem>();
            //return WeaponList.DIABLO2WEAPONS;
        }
    }

    public class Cultist : Monster
    {

        public Cultist()
            : base("Cultist", "https://i.pinimg.com/originals/26/46/ab/2646ab16302d0bd5d93628854ca1ee61.png")
        {
        }
    }

    public class CorruptRogueArcher : Monster
    {

        public CorruptRogueArcher()
            : base("CorruptRogueArcher", "https://static.wikia.nocookie.net/diabloGamepedia/images/8/88/DarkRanger%28DiabloII%29.gif", pMonsterTypes: new List<MonsterType> {
                    MonsterType.DEMON,
                    MonsterType.HUMANOID
            })
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem>();
            //return WeaponList.DIABLO2WEAPONS;
        }
    }

    public class Fallen : Monster
    {

        public Fallen()
            : base("Fallen", "https://static.wikia.nocookie.net/diabloGamepedia/images/d/db/Fallen%28DiabloII%29.gif", pMonsterType: MonsterType.DEMON)
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem>();
            //return WeaponList.DIABLO2WEAPONS;
        }
    }

    public class Boar : Monster
    {

        public Boar()
            : base("Boar", "https://www.drawize.com/drawings/images/52fdc3wild-boar?width=1200", pMonsterType: MonsterType.BEAST)
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.boarFang
                };
        }
    }

    public class Gremlin : Monster
    {

        public Gremlin()
            : base("Gremlin", "https://cdn1.tedsby.com/tb/large/storage/3/6/3/363895/collectible-fantasy-creature-gremlin-mr-grimm.jpg")
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.apprenticeStaff
                };
        }
    }

    public class Whelp : Monster
    {

        public Whelp()
            : base("Whelp", "https://cdn2.warcraftpets.com/images/pets/big/greenWhelp.vb00ce173b1da0717e09aa4e6f336b35da109bf14.jpg", pMonsterType: MonsterType.DRAGON)
        {
        }
    }

    public class Zombie : Monster
    {

        public Zombie()
            : base("Zombie", "https://i.pinimg.com/originals/db/6c/ce/db6ccebffed0da50cbafb8b895673ead.png", pMonsterType: MonsterType.UNDEAD)
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.bone,
                    //WeaponList.rustedSword
                };
        }
    }

    public class GiantFrog : Monster
    {

        public GiantFrog()
            : base("GiantFrog", "https://4.bp.blogspot.com/-8iZ33JJtEQ4/WHNjFa587HI/AAAAAAAABFk/pUsoNTzuLkA02nPgkb-38rxKSwWP5WPQCLcB/s1600/giantToad.jpg", pMonsterType: MonsterType.BEAST)
        {
        }
    }

    public class Wolf : Monster
    {

        public Wolf()
            : base("Wolf", "https://i.pinimg.com/originals/4c/e9/ca/4ce9caa4f8fb034792434f4280a95722.png", pMonsterType: MonsterType.BEAST)
        {
        }
    }

    public class Troglodyte : Monster
    {

        public Troglodyte()
            : base("Troglodyte", "https://64.media.tumblr.com/6fc9f2a3c8c1a14408ad1a9faa13d24b/9f240ebb94c93d0b-62/s540x810/9e249d4056e1b5c29268b621fb02174b2361b740.png")
        {
        }
    }

    public class Boggard : Monster
    {

        public Boggard()
            : base("Boggard", "https://guildberkeley.files.wordpress.com/2020/03/boggard2-1.jpg")
        {
        }
    }

    public class ManAtArms : Monster
    {

        public ManAtArms()
            : base("ManAtArms", "https://shop.bestsoldiershop.com/WebRoot/StoreIT5/Shops/14739/5662/B7BF/F14D/9483/A8E5/3E95/9311/21C9/TB54025MENATARMS14th24TINBERLIN.jpg")
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.armingSword
                };
        }
    }

    public class Gladiator : Monster
    {

        public Gladiator()
            : base("Gladiator", "https://i.pinimg.com/originals/66/cb/f8/66cbf892eb5f0c8e829bf7e3cbff5e00.jpg")
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.gladius
                };
        }
    }

    public class Werebear : Monster
    {

        public Werebear()
            : base("Werebear", "https://cdna.artstation.com/p/assets/images/images/000/239/056/large/grzegorz-pedrycz-warewolf.jpg?1412699713", pMonsterTypes: new List<MonsterType> {
                    MonsterType.MAGIC_CREATURE,
                    MonsterType.HUMANOID
            })
        {
        }
    }

    public class Troll : Monster
    {

        public Troll()
            : base("Troll", "https://i.pinimg.com/originals/12/72/95/1272957523d7e212eb3bd9f8dc7491de.png", MonsterType.MAGIC_CREATURE)
        {
        }
    }

    public class Lillend : Monster
    {

        public Lillend()
            : base("Lillend", "https://64.media.tumblr.com/980e9ba4cd8ee3ff85bc9a109b903d22/tumblrInlinePqnmp83F7y1robfbt640.png", MonsterType.MAGIC_CREATURE)
        {
        }
    }

    public class KillerRabbit : Monster
    {

        public KillerRabbit()
            : base("KillerRabbit", "https://static.wikia.nocookie.net/montypython/images/d/dd/KillerRabbit.JPG/revision/latest?cb=20070904000613", pMonsterType: MonsterType.BEAST)
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.holyHandGrenade
                };
        }
    }

    public class EdgeLord : Monster
    {

        public EdgeLord()
            : base("EdgeLord", "https://preview.redd.it/v32cdps3yeqy.png?auto=webp&s=5d7627e25ef7821fe806aced07f40c2927ba575e")
        {
        }

        public override List<LootItem> GetItemDrops()
        {
            return new List<LootItem> {
                    //WeaponList.katana,
                    //WeaponList.apprenticeStaff
                };
        }
    }

    public class Lyander : Monster
    {

        public Lyander()
            : base("Lyander", "https://cdna.artstation.com/p/assets/images/images/033/743/432/large/william-hallett-tilanis-revision.jpg?1610461935")
        {
        }
    }

    public class Bebilith : Monster
    {

        public Bebilith()
            : base("Bebilith", "https://dnd35.files.wordpress.com/2016/04/bebilith.png", MonsterType.MAGIC_CREATURE)
        {
        }
    }

    public class Angel : Monster
    {

        public Angel()
            : base("Angel", "https://i.pinimg.com/originals/3c/73/a6/3c73a62d3b71b53672c86bc77fa1c472.png", MonsterType.MAGIC_CREATURE)
        {
        }
    }

    public class GorillaDire : Monster
    {

        public GorillaDire()
            : base("GorillaDire", "https://pathfinderwiki.com/mediawiki/images/1/15/Gorilla.jpg", pMonsterType: MonsterType.BEAST)
        {
        }
    }

    public class TRex : Monster
    {

        public TRex()
            : base("TRex", "https://cdn.mos.cms.futurecdn.net/acwNent8xhiprPHCYj8bvK-1200-80.jpg", pMonsterType: MonsterType.BEAST)
        {
        }
    }

    public class Brightwing : Monster
    {

        public Brightwing()
            : base("Brightwing", "https://static.wikia.nocookie.net/wowpedia/images/9/9c/BrightwingArt.jpg/revision/latest/scale-to-width-down/1000?cb=20190902141719", MonsterType.MAGIC_CREATURE)
        {
        }
    }

    public class Hydra : Monster
    {

        public Hydra()
            : base("Hydra", "https://img1.goodfon.com/original/1920x1080/4/37/fantastika-monstr-golovy.jpg", MonsterType.MAGIC_CREATURE)
        {
        }
    }

    public class GreaterDemon : Monster
    {

        public GreaterDemon()
            : base("GreaterDemon", "https://cdnb.artstation.com/p/assets/images/images/028/536/711/medium/masaki-hayashi-demon-final.jpg?1594752931", pMonsterType: MonsterType.DEMON)
        {
        }
    }

    public class Imp : Monster
    {

        public Imp()
            : base("Imp", "https://cdna.artstation.com/p/assets/images/images/032/096/084/large/aleksandr-golubev-cute-devil-girl2-2.jpg?1605471333", pMonsterType: MonsterType.DEMON)
        {
        }
    }

    public class Succubus : Monster
    {

        public Succubus()
            : base("Succubus", "https://cdna.artstation.com/p/assets/images/images/027/194/238/large/bbang-q-09.jpg?1590916369", pMonsterType: MonsterType.DEMON)
        {
        }
    }

    public class NinjaBrian : Monster
    {

        public NinjaBrian()
            : base("NinjaBrian", "https://pbs.twimg.com/media/D8a0hsiUcAY-1J-?format=jpg&name=large", true)
        {
        }
    }

    public class DoomGuy : Monster
    {

        public DoomGuy()
            : base("DoomGuy", "https://static.wikia.nocookie.net/fatecrossover/images/2/2e/DoomEternal.jpg.jpg", true)
        {
        }
    }

    public class TheLichKing : Monster
    {

        public TheLichKing()
            : base("TheLichKing", "https://static.wikia.nocookie.net/wowwiki/images/5/5c/Fanart-0827-large.jpg/", pBossMonster: true, pMonsterTypes: new List<MonsterType> {
                    MonsterType.UNDEAD,
                    MonsterType.HUMANOID
            })
        {
        }
    }

    public class Diablo : Monster
    {

        public Diablo()
            : base("Diablo", "https://static.wikia.nocookie.net/diablo/images/4/42/Diablo.gif", pBossMonster: true, pMonsterType: MonsterType.DEMON)
        {
        }
    }

    public class Deathwing : Monster
    {

        public Deathwing()
            : base("Deathwing", "https://static.wikia.nocookie.net/wowwiki/images/1/10/Earthwarder.png/revision/latest/scale-to-width-down/720?cb=20140503155011", pBossMonster: true, pMonsterType: MonsterType.DRAGON)
        {
        }
    }
}