namespace NeoMapleStory.Game.Data
{
    public interface IMapleDataFileEntry : IMapleDataEntry
    {
        new int Offset { get; set; }
    }
}