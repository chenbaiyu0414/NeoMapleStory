namespace NeoMapleStory.Game.Inventory
{
    public class MapleInventoryType
    {
        public MapleInventoryType()
        {
        }

        public MapleInventoryType(byte value)
        {
            Value = value;
        }

        public static MapleInventoryType Undefined { get; } = new MapleInventoryType(0);
        public static MapleInventoryType Equip { get; } = new MapleInventoryType(1);
        public static MapleInventoryType Use { get; } = new MapleInventoryType(2);
        public static MapleInventoryType Setup { get; } = new MapleInventoryType(3);
        public static MapleInventoryType Etc { get; } = new MapleInventoryType(4);
        public static MapleInventoryType Cash { get; } = new MapleInventoryType(5);
        public static MapleInventoryType Equipped { get; } = new MapleInventoryType(6);

        public byte Value { get; }

        public static MapleInventoryType[] TypeList { get; } = {Undefined, Equip, Use, Setup, Etc, Cash, Equipped};

        //public short GetBitfieldEncoding()
        //{
        //    return (short)(2 << Value);
        //}

        public static MapleInventoryType GetByType(byte type)
        {
            switch (type)
            {
                case 0:
                    return Undefined;
                case 1:
                    return Equip;
                case 2:
                    return Use;
                case 3:
                    return Setup;
                case 4:
                    return Etc;
                case 5:
                    return Cash;
                case 6:
                    return Equipped;
            }
            return null;
        }

        public static MapleInventoryType GetByWzName(string name)
        {
            switch (name)
            {
                case "Install":
                    return Setup;
                case "Consume":
                    return Use;
                case "Etc":
                    return Etc;
                case "Cash":
                    return Cash;
                case "Pet":
                    return Cash;
                default:
                    return Undefined;
            }
        }

        public static bool operator ==(MapleInventoryType type1, MapleInventoryType type2) => type1.Value == type2.Value;

        public static bool operator !=(MapleInventoryType type1, MapleInventoryType type2) => type1.Value != type2.Value;

        public bool Equals(MapleInventoryType other) => Value == other.Value;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MapleInventoryType) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}