namespace NeoMapleStory.Game.Shop
{
    public class MapleShopItem
    {
        public MapleShopItem(short buyable, int itemId, int price)
        {
            Buyable = buyable;
            ItemId = itemId;
            Price = price;
        }

        public short Buyable { get; private set; }
        public int ItemId { get; private set; }
        public int Price { get; private set; }
        public long RefreshTime { get; private set; } = 0;
        public short AvailibleCount { get; set; }

        public void DecAvailible()
        {
            AvailibleCount--;
        }

        public void IncAvailible()
        {
            AvailibleCount++;
        }
    }
}