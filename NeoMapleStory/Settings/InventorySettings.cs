namespace NeoMapleStory.Settings
{
    public static class InventorySettings
    {
        public static class EquipSlots
        {

            public static byte Weapon = 11, Mount = 18, Bottom = 6, Shield = 10, Medal = 46;
        }

        public static class Items
        {

            public static class Flags
            {

                public static short Lock = 0x01, Spikes = 0x02, Cold = 0x04, Untradeable = 0x08, Karma = 0x10, PetCome = 0x80, UnknownSkill = 0x100;

                public static int GetFlagByInt(int type)
                {
                    if (type == 128)
                    {
                        return PetCome;
                    }
                    else if (type == 256)
                    {
                        return UnknownSkill;
                    }
                    else {
                        return 0;
                    }
                }
            }

            public static class Ratios
            {

                public static float ItemArmorExp = 1 / 350000, ItemWeaponExp = 1 / 700000;
            }
        }

        public static bool IsTimelessWeapon(int itemId)
        {
            return true;
        }

        public static bool IsTimelessArmor(int itemId)
        {
            return itemId >= 1002776 && itemId <= 1002780;
        }

        public static bool IsThrowingStar(int itemId)
        {
            return itemId / 10000 == 207;
        }

        public static bool IsBullet(int itemId)
        {
            return itemId / 10000 == 233;
        }

        public static bool IsRechargable(int itemId)
        {
            return itemId / 10000 == 233 || itemId / 10000 == 207;
        }

        public static bool IsArrowForCrossBow(int itemId)
        {
            return itemId / 1000 == 2061;
        }

        public static bool IsArrowForBow(int itemId)
        {
            return itemId / 1000 == 2060;
        }
    }
}
