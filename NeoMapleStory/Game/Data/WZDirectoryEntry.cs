using System.Collections.Generic;

namespace NeoMapleStory.Game.Data
{
    public class WzDirectoryEntry : WzEntry, IMapleDataDirectoryEntry
    {
        private readonly Dictionary<string, IMapleDataEntry> m_mEntries = new Dictionary<string, IMapleDataEntry>();
        private readonly List<IMapleDataFileEntry> m_mFiles = new List<IMapleDataFileEntry>();
        private readonly List<IMapleDataDirectoryEntry> m_mSubDirs = new List<IMapleDataDirectoryEntry>();

        public WzDirectoryEntry(string name, int size, int checksum, IMapleDataEntity parent)
            : base(name, size, checksum, parent)
        {
        }

        public WzDirectoryEntry()
            : base(null, 0, 0, null)
        {
        }

        public IMapleDataEntry GetEntry(string name) => m_mEntries[name];


        public List<IMapleDataFileEntry> GetFiles() => m_mFiles;


        public List<IMapleDataDirectoryEntry> GetSubDirectories() => m_mSubDirs;

        public void AddDirectory(IMapleDataDirectoryEntry dir)
        {
            m_mSubDirs.Add(dir);
            if (m_mEntries.ContainsKey(dir.Name))
                m_mEntries[dir.Name] = dir;
            else
                m_mEntries.Add(dir.Name, dir);
        }

        public void AddFile(IMapleDataFileEntry fileEntry)
        {
            m_mFiles.Add(fileEntry);
            if (m_mEntries.ContainsKey(fileEntry.Name))
                m_mEntries[fileEntry.Name] = fileEntry;
            else
                m_mEntries.Add(fileEntry.Name, fileEntry);
        }
    }
}