namespace NeoMapleStory.Game.Data
{
    public interface IMapleDataEntry : IMapleDataEntity
    {
        int Size { get; }
        int Checksum { get; }
        int Offset { get; }
    }
}