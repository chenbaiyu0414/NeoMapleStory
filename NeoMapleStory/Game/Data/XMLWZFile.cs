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
            foreach (string filepath in Directory.GetFiles(lroot))
            {
                if (Directory.Exists(filepath) && !filepath.EndsWith(".img"))
                {
                    WzDirectoryEntry newDir = new WzDirectoryEntry(filepath, 0, 0, wzdir);
                    wzdir.AddDirectory(newDir);
                    FillMapleDataEntitys(filepath, newDir);
                }
                else if (filepath.EndsWith(".xml"))
                {
                    // get the real size here?
                    wzdir.AddFile(new WzFileEntry(Path.GetFileNameWithoutExtension(filepath), 0, 0, wzdir));
                }
            }
        }

        public IMapleData GetData(string path)
        {
            string dataPath = $"{_mRoot}\\{path}.xml";

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
