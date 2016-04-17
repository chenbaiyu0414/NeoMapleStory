using System;

namespace NeoMapleStory.Game.Data
{
     public class MapleDataProviderFactory
    {
        private static readonly string MWzPath = $"{Environment.CurrentDirectory}\\WZ\\";

        private static IMapleDataProvider GetWz(string path, bool provideImages)
        {

            //if (Path.GetFileName(m_wzPath).EndsWith(".wz") && File.GetAttributes(m_wzPath) != FileAttributes.Directory)
            //{
            //    try
            //    {
            //        return new WZFile(path, provideImages);
            //    }
            //    catch (IOException e)
            //    {

            //    }
            //}
            //else
            //{
            return new XmlwzFile($"{MWzPath}{path}");
            //}
        }

        public static IMapleDataProvider GetDataProvider(string filein)
        {
            return GetWz(filein, false);
        }

        public static IMapleDataProvider GetImageProvidingDataProvider(string filein)
        {
            return GetWz(filein, true);
        }

        //public static File fileInWZPath(string filename)
        //{
        //    return new File(m_wzPath, filename);
        //}
    }
}
