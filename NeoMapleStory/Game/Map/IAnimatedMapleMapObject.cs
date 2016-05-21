namespace NeoMapleStory.Game.Map
{
    public interface IAnimatedMapleMapObject : IMapleMapObject
    {
        int Stance { get; set; }
        bool IsFacingLeft { get; }
    }
}