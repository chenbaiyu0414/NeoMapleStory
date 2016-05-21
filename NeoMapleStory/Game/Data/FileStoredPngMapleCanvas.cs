using System.Drawing;

namespace NeoMapleStory.Game.Data
{
    public class FileStoredPngMapleCanvas : IMapleCanvas
    {
        private readonly string m_mPath;

        public FileStoredPngMapleCanvas(int width, int height, string path)
        {
            Width = width;
            Height = height;
            m_mPath = path;
        }

        public int Height { get; private set; }
        public Image Picture { get; private set; }
        public int Width { get; private set; }

        private void LoadImageIfNecessary()
        {
            if (Picture == null)
            {
                Picture = Image.FromFile(m_mPath);
                Width = Picture.Width;
                Height = Picture.Height;
            }
        }
    }
}