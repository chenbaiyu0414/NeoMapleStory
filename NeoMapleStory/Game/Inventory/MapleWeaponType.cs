namespace NeoMapleStory.Game.Inventory
{
    public class MapleWeaponType
    {
        public MapleWeaponType(double maxDamageMultiplier)
        {
            DamageMultiplier = maxDamageMultiplier;
        }

        public static MapleWeaponType NotAWeapon { get; } = new MapleWeaponType(0);
        public static MapleWeaponType Bow { get; } = new MapleWeaponType(3.4);
        public static MapleWeaponType Claw { get; } = new MapleWeaponType(3.6);
        public static MapleWeaponType Dagger { get; } = new MapleWeaponType(4);
        public static MapleWeaponType Crossbow { get; } = new MapleWeaponType(3.6);
        public static MapleWeaponType Axe1H { get; } = new MapleWeaponType(4.4);
        public static MapleWeaponType Sword1H { get; } = new MapleWeaponType(4.0);
        public static MapleWeaponType Blunt1H { get; } = new MapleWeaponType(4.4);
        public static MapleWeaponType Axe2H { get; } = new MapleWeaponType(4.8);
        public static MapleWeaponType Sword2H { get; } = new MapleWeaponType(4.6);
        public static MapleWeaponType Blunt2H { get; } = new MapleWeaponType(4.8);
        public static MapleWeaponType PoleArm { get; } = new MapleWeaponType(5.0);
        public static MapleWeaponType Spear { get; } = new MapleWeaponType(5.0);
        public static MapleWeaponType Staff { get; } = new MapleWeaponType(3.6);
        public static MapleWeaponType Wand { get; } = new MapleWeaponType(3.6);
        public static MapleWeaponType Knuckle { get; } = new MapleWeaponType(4.8);
        public static MapleWeaponType Gun { get; } = new MapleWeaponType(3.6);

        public double DamageMultiplier { get; private set; }
    }
}