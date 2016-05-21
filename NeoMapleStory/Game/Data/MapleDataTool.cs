using System.Drawing;

namespace NeoMapleStory.Game.Data
{
    public class MapleDataTool
    {
        public static string GetString(IMapleData data) => (string) data.Data;


        public static string GetString(IMapleData data, string def)
            => data == null || data.Data == null ? def : (string) data.Data;


        public static string GetString(string path, IMapleData data) => GetString(data.GetChildByPath(path));


        public static string GetString(string path, IMapleData data, string def)
            => GetString(data.GetChildByPath(path), def);


        public static double GetDouble(IMapleData data) => (double) data.Data;


        public static float GetFloat(IMapleData data) => (float) data.Data;


        public static int GetInt(IMapleData data) => (int) data.Data;

        public static short GetShort(IMapleData data) => (short) data.Data;

        public static int GetInt(IMapleData data, int def) => (int?) data?.Data ?? def;

        public static short GetShort(IMapleData data, short def) => (short?) data?.Data ?? def;

        public static int GetInt(string path, IMapleData data) => ConvertToInt(path, data);


        public static int ConvertToInt(IMapleData data)
            => data.GetType() == MapleDataType.String ? int.Parse(GetString(data)) : GetInt(data);


        public static int ConvertToInt(string path, IMapleData data)
        {
            var d = data.GetChildByPath(path);
            if (d.GetType() == MapleDataType.String)
                return int.Parse(GetString(d));
            return GetInt(d);
        }

        public static int GetInt(string path, IMapleData data, int def) => ConvertToInt(path, data, def);


        public static int ConvertToInt(string path, IMapleData data, int def)
        {
            var d = data.GetChildByPath(path);
            if (d == null)
            {
                return def;
            }
            if (d.GetType() == MapleDataType.String)
            {
                try
                {
                    return int.Parse(GetString(d));
                }
                catch
                {
                    return def;
                }
            }
            return GetInt(d, def);
        }

        public static Image GetImage(IMapleData data) => ((IMapleCanvas) data.Data).Picture;


        public static Point GetPoint(IMapleData data) => (Point) data.Data;


        public static Point GetPoint(string path, IMapleData data) => GetPoint(data.GetChildByPath(path));


        public static Point GetPoint(string path, IMapleData data, Point def)
        {
            var pointData = data.GetChildByPath(path);
            if (pointData == null)
                return def;

            return GetPoint(pointData);
        }

        public static string GetFullDataPath(IMapleData data)
        {
            var path = "";
            IMapleDataEntity myData = data;
            while (myData != null)
            {
                path = myData.Name + "/" + path;
                myData = myData.Parent;
            }
            return path.Substring(0, path.Length - 1);
        }
    }
}