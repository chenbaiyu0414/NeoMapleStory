namespace NeoMapleStory.Game.Inventory
{
    public  class CashItem
    {
        public int Sn { get; private set; }
        public int ItemId { get; private set; }
        public short Count { get; private set; }
        public int Price { get; private set; }
        public int Period { get; private set; }
        public int Gender { get; private set; }
        public bool IsOnSale { get; private set; }

        public CashItem(int SN, int itemId,short count, int price, int period, int gender, bool isOnSale)
        {
            Sn = SN;
            ItemId = itemId;
            Count = count;
            Price = price;
            Period = period;
            Gender = gender;
            IsOnSale = isOnSale;
        }
    }
}
