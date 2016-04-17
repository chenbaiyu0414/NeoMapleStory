using System.Collections.Generic;

namespace NeoMapleStory.Game.Data
{
    public  interface IMapleDataDirectoryEntry: IMapleDataEntry
    {
        List<IMapleDataDirectoryEntry> GetSubDirectories();

        List<IMapleDataFileEntry> GetFiles();

        IMapleDataEntry GetEntry(string name);

    }
}
