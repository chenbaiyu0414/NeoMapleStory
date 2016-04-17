using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace NeoMapleStory.Game.Data
{
    public class XmlDomMapleData : IMapleData
    {
        private readonly XmlNode _mNode;
        private string _mImageDataDir;

        public XmlDomMapleData(string xmlpath)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(xmlpath);
            _mNode = xmldoc.FirstChild.NextSibling;
            _mImageDataDir = Path.GetFullPath(xmlpath);

        }

        private XmlDomMapleData(XmlNode node)
        {
            _mNode = node;
        }


        public IMapleData GetChildByPath(string path)
        {
            string[] segments = path.Split('/');

            if (segments[0].Equals(".."))
            {
                return ((IMapleData)Parent).GetChildByPath(path.Substring(path.IndexOf("/") + 1));
            }

            XmlNode myNode = _mNode;

            for (int x = 0; x < segments.Length; x++)
            {
                XmlNodeList childNodes = myNode.ChildNodes;
                bool foundChild = false;
                for (int i = 0; i < childNodes.Count; i++)
                {
                    XmlNode childNode = childNodes.Item(i);
                    if (childNode.NodeType == XmlNodeType.Element && childNode.Attributes.GetNamedItem("name").Value.Equals(segments[x]))
                    {
                        myNode = childNode;
                        foundChild = true;
                        break;
                    }
                }
                if (!foundChild)
                {
                    return null;
                }
            }
            XmlDomMapleData ret = new XmlDomMapleData(myNode);
            ret._mImageDataDir = new DirectoryInfo($"{_mImageDataDir}\\{Name}\\{path}").Parent.FullName;
            return ret;
        }


        public List<IMapleData> Children
        {
            get
            {
                List<IMapleData> ret = new List<IMapleData>();
                XmlNodeList childNodes = _mNode.ChildNodes;
                for (int i = 0; i < childNodes.Count; i++)
                {
                    XmlNode childNode = childNodes.Item(i);
                    if (childNode.NodeType == XmlNodeType.Element)
                    {
                        XmlDomMapleData child = new XmlDomMapleData(childNode);
                        child._mImageDataDir = new DirectoryInfo($"{_mImageDataDir}\\{Name}").FullName;
                        ret.Add(child);
                    }
                }
                return ret;
            }
        }


        public object Data
        {
            get
            {
                var attributes = _mNode.Attributes;
                MapleDataType type = GetType();
                switch (type)
                {
                    case MapleDataType.Double:
                    case MapleDataType.Float:
                    case MapleDataType.Int:
                    case MapleDataType.Short:
                    case MapleDataType.String:
                    case MapleDataType.Uol:
                        {
                            string value = attributes.GetNamedItem("value").Value;
                            switch (type)
                            {
                                case MapleDataType.Double:
                                    return double.Parse(value);
                                case MapleDataType.Float:
                                    return float.Parse(value);
                                case MapleDataType.Int:
                                    return int.Parse(value);
                                case MapleDataType.Short:
                                    return short.Parse(value);
                                case MapleDataType.String:
                                case MapleDataType.Uol:
                                    return value;
                            }
                        }
                        break;
                    case MapleDataType.Vector:
                        {
                            string x = attributes.GetNamedItem("x").Value;
                            string y = attributes.GetNamedItem("y").Value;
                            return new Point(int.Parse(x), int.Parse(y));
                        }
                    case MapleDataType.Canvas:
                        {
                            string width = attributes.GetNamedItem("width").Value;
                            string height = attributes.GetNamedItem("height").Value;
                            return new FileStoredPngMapleCanvas(int.Parse(width), int.Parse(height), $"{_mImageDataDir}\\{Name}.png");
                        }
                }
                return null;
            }
        }


        public new MapleDataType GetType()
        {
            switch (_mNode.Name)
            {
                case "imgdir":
                    return MapleDataType.Property;
                case "canvas":
                    return MapleDataType.Canvas;
                case "convex":
                    return MapleDataType.Convex;
                case "sound":
                    return MapleDataType.Sound;
                case "uol":
                    return MapleDataType.Uol;
                case "double":
                    return MapleDataType.Double;
                case "float":
                    return MapleDataType.Float;
                case "int":
                    return MapleDataType.Int;
                case "short":
                    return MapleDataType.Short;
                case "string":
                    return MapleDataType.String;
                case "vector":
                    return MapleDataType.Vector;
                case "null":
                    return MapleDataType.Img_0X00;
            }
            return default(MapleDataType);
        }

        public IEnumerator<IMapleData> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IMapleDataEntity Parent
        {
            get
            {
                XmlNode parentNode = _mNode.ParentNode;
                if (parentNode.NodeType == XmlNodeType.Document)
                {
                    return null; // can't traverse outside the img file - TODO is this a problem?
                }
                XmlDomMapleData parentData = new XmlDomMapleData(parentNode);
                parentData._mImageDataDir = new DirectoryInfo(_mImageDataDir).Parent.FullName;
                return parentData;
            }
        }


        public string Name => _mNode.Attributes.GetNamedItem("name").Value;
    }
}
