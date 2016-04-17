using System.Drawing;

namespace NeoMapleStory.Game.Data
{
    public interface IMapleCanvas
    {
        int Height { get; }
        int Width { get; }
        Image Picture { get; }
    }
}
