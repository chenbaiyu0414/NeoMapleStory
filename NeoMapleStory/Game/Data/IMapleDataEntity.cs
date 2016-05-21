namespace NeoMapleStory.Game.Data
{
    public interface IMapleDataEntity
    {
        string Name { get; }

        IMapleDataEntity Parent { get; }
    }
}