namespace NeoMapleStory.Game.Client.AntiCheat
{
    public class CheatingOffense
    {
        public static CheatingOffense Fastattack { get; } = new CheatingOffense(1, 60000, 300);
        public static CheatingOffense MoveMonsters { get; } = new CheatingOffense();
        public static CheatingOffense AlwaysOneHit { get; } = new CheatingOffense();
        public static CheatingOffense Tubi { get; } = new CheatingOffense();
        public static CheatingOffense FastHpRegen { get; } = new CheatingOffense();
        public static CheatingOffense FastMpRegen { get; } = new CheatingOffense(1, 60000, 500);
        public static CheatingOffense SameDamage { get; } = new CheatingOffense(10, 300000, 20);
        public static CheatingOffense AttackWithoutGettingHit = new CheatingOffense();
        public static CheatingOffense HighDamage { get; } = new CheatingOffense(10, 300000L);
        public static CheatingOffense AttackFarawayMonster { get; } = new CheatingOffense(5);
        public static CheatingOffense RegenHighHp { get; } = new CheatingOffense(50);
        public static CheatingOffense RegenHighMp { get; } = new CheatingOffense(50);
        public static CheatingOffense Itemvac { get; } = new CheatingOffense(5);
        public static CheatingOffense ShortItemvac { get; } = new CheatingOffense(2);
        public static CheatingOffense UsingFarawayPortal { get; } = new CheatingOffense(30, 300000);
        public static CheatingOffense FastTakeDamage { get; } = new CheatingOffense(1);
        public static CheatingOffense FastMove { get; } = new CheatingOffense(1, 60000);
        public static CheatingOffense HighJump { get; } = new CheatingOffense(1, 60000);
        public static CheatingOffense MismatchingBulletcount { get; } = new CheatingOffense(50);
        public static CheatingOffense EtcExplosion { get; } = new CheatingOffense(50, 300000);
        public static CheatingOffense FastSummonAttack = new CheatingOffense();
        public static CheatingOffense AttackingWhileDead { get; } = new CheatingOffense(10, 300000);
        public static CheatingOffense UsingUnavailableItem { get; } = new CheatingOffense(10, 300000);
        public static CheatingOffense FamingSelf { get; } = new CheatingOffense(10, 300000); // purely for marker reasons (appears in the database)
        public static CheatingOffense FamingUnder15 { get; } = new CheatingOffense(10, 300000);
        public static CheatingOffense ExplodingNonexistant = new CheatingOffense();
        public static CheatingOffense SummonHack = new CheatingOffense();
        public static CheatingOffense HealAttackingUndead { get; } = new CheatingOffense(1, 60000, 5);
        public static CheatingOffense CooldownHack { get; } = new CheatingOffense(10, 300000, 10);
        public static CheatingOffense MobInstantDeathHack { get; } = new CheatingOffense(10, 300000, 5);

        public int Points { get; private set; }
        public long ValidityDuration { get; private set; }
        public int AutobanCount { get; private set; }
        public bool Enabled { get; set; } = true;


        public bool ShouldAutoban(int count)
        {
            if (AutobanCount == -1)
            {
                return false;
            }
            return count > AutobanCount;
        }


        private CheatingOffense()
            : this(1)
        {
        }

        private CheatingOffense(int points)
            : this(points, 60000)
        {

        }

        private CheatingOffense(int points, long validityDuration)
            : this(points, validityDuration, -1)
        {

        }

        private CheatingOffense(int points, long validityDuration, int autobancount)
            : this(points, validityDuration, autobancount, true)
        {

        }

        private CheatingOffense(int points, long validityDuration, int autobancount, bool enabled)
        {
            this.Points = points;
            this.ValidityDuration = validityDuration;
            this.AutobanCount = autobancount;
            this.Enabled = enabled;
        }
    }
}
