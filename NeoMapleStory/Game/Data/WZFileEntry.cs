namespace NeoMapleStory.Game.Data
{
    public class WzFileEntry : WzEntry, IMapleDataFileEntry
    {
        public new int Offset { get; set; }

        public WzFileEntry(string name, int size, int checksum, IMapleDataEntity parent)
            : base(name, size, checksum, parent)
        {

        }

    }
}
