namespace NeoMapleStory.Game.Data
{
    public interface IMapleDataProvider
    {
        IMapleData GetData(string path);

        IMapleDataDirectoryEntry GetRoot();
    }
}