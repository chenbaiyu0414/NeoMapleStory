namespace NeoMapleStory.Game.Data
{
    public class WzEntry : IMapleDataEntry
    {
        public string Name { get; private set; }
        public int Size { get; private set; }
        public int Checksum { get; private set; }
        public int Offset { get; private set; }
        public IMapleDataEntity Parent { get; private set; }

        public WzEntry(string name, int size, int checksum, IMapleDataEntity parent)
        {
            Name = name;
            Size = size;
            Checksum = checksum;
            Parent = parent;
        }
    }
}
