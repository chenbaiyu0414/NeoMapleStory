namespace NeoMapleStory.Game.Movement
{
    public interface ILifeMovement : ILifeMovementFragment
    {
        byte Newstate { get; }
        short Duration { get; }
        byte Type { get; }
    }
}
