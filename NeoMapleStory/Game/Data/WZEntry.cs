namespace NeoMapleStory.Game.Data
{
    public class WzEntry : IMapleDataEntry
    {
        public WzEntry(string name, int size, int checksum, IMapleDataEntity parent)
        {
            Name = name;
            Size = size;
            Checksum = checksum;
            Parent = parent;
        }

        public string Name { get; }
        public int Size { get; }
        public int Checksum { get; }
        public int Offset { get; private set; }
        public IMapleDataEntity Parent { get; }
    }
}