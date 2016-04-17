using System.Collections.Generic;

namespace NeoMapleStory.Game.Data
{
    public class WzDirectoryEntry : WzEntry, IMapleDataDirectoryEntry
    {
        private readonly List<IMapleDataDirectoryEntry> _mSubDirs = new List<IMapleDataDirectoryEntry>();
        private readonly List<IMapleDataFileEntry> _mFiles = new List<IMapleDataFileEntry>();
        private readonly Dictionary<string, IMapleDataEntry> _mEntries = new Dictionary<string, IMapleDataEntry>();

        public WzDirectoryEntry(string name, int size, int checksum, IMapleDataEntity parent)
    : base(name, size, checksum, parent)
        {

        }

        public WzDirectoryEntry()
            : base(null, 0, 0, null)
        {

        }

        public void AddDirectory(IMapleDataDirectoryEntry dir)
        {
            _mSubDirs.Add(dir);
            _mEntries.Add(dir.Name, dir);
        }

        public void AddFile(IMapleDataFileEntry fileEntry)
        {
            _mFiles.Add(fileEntry);
            _mEntries.Add(fileEntry.Name, fileEntry);
        }

        public IMapleDataEntry GetEntry(string name) => _mEntries[name];


        public List<IMapleDataFileEntry> GetFiles() => _mFiles;


        public List<IMapleDataDirectoryEntry> GetSubDirectories() => _mSubDirs;

    }
}
