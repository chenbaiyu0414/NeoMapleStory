using System;
using System.IO;

namespace NeoMapleStory.Game.Data
{
     public class XmlwzFile : IMapleDataProvider
    {
        private readonly string _mRoot;
        private readonly WzDirectoryEntry _mRootForNavigation;

        public XmlwzFile(string filepath)
        {
            _mRoot = filepath;
            _mRootForNavigation = new WzDirectoryEntry(filepath, 0, 0, null);
            FillMapleDataEntitys(_mRoot, _mRootForNavigation);
        }

        private void FillMapleDataEntitys(string lroot, WzDirectoryEntry wzdir)
        {
            foreach (var filepath in new DirectoryInfo(lroot).GetFileSystemInfos())
            {
                if (Directory.Exists(filepath.FullName) && !filepath.Name.EndsWith(".img"))
                {
                    WzDirectoryEntry newDir = new WzDirectoryEntry(filepath.Name, 0, 0, wzdir);
                    wzdir.AddDirectory(newDir);
                    FillMapleDataEntitys(filepath.FullName, newDir);
                }
                else if (filepath.Name.EndsWith(".xml"))
                {
                    // get the real size here?
                    wzdir.AddFile(new WzFileEntry(Path.GetFileNameWithoutExtension(filepath.Name), 0, 0, wzdir));
                }
            }
        }

        public IMapleData GetData(string path)
        {
            string dataPath = $"{_mRoot}//{path}.xml";

            if (!File.Exists(dataPath))
            {
                throw new Exception($"文件不存在 路径:{dataPath}");
            }

            XmlDomMapleData domMapleData = new XmlDomMapleData(dataPath);
           
            return domMapleData;
        }


        public IMapleDataDirectoryEntry GetRoot()
        {
            return _mRootForNavigation;
        }
    }
}
