using System.Drawing;

namespace NeoMapleStory.Game.Data
{
    public class FileStoredPngMapleCanvas : IMapleCanvas
    {
        public int Height { get; private set; }
        public Image Picture { get; private set; }
        public int Width { get; private set; }

        private readonly string _mPath;

        public FileStoredPngMapleCanvas(int width, int height, string path)
        {
            Width = width;
            Height = height;
            _mPath = path;
        }

        private void LoadImageIfNecessary()
        {
            if (Picture == null)
            {
                    Picture = Image.FromFile(_mPath);
                    Width = Picture.Width;
                    Height = Picture.Height;
            }
        }
    }
}
