using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace NeoMapleStory.Game.Data
{
    public class XmlDomMapleData : IMapleData
    {
        private readonly XmlNode m_mNode;
        private string m_mImageDataDir;

        public XmlDomMapleData(string xmlpath)
        {
            var xmldoc = new XmlDocument();
            xmldoc.Load(xmlpath);
            m_mNode = xmldoc.FirstChild.NextSibling;
            m_mImageDataDir = Path.GetFullPath(xmlpath);
        }

        private XmlDomMapleData(XmlNode node)
        {
            m_mNode = node;
        }


        public IMapleData GetChildByPath(string path)
        {
            var segments = path.Split('/');

            if (segments[0].Equals(".."))
            {
                return ((IMapleData) Parent).GetChildByPath(path.Substring(path.IndexOf("/") + 1));
            }

            var myNode = m_mNode;

            for (var x = 0; x < segments.Length; x++)
            {
                var childNodes = myNode.ChildNodes;
                var foundChild = false;
                for (var i = 0; i < childNodes.Count; i++)
                {
                    var childNode = childNodes.Item(i);
                    if (childNode.NodeType == XmlNodeType.Element &&
                        childNode.Attributes.GetNamedItem("name").Value.Equals(segments[x]))
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
            var ret = new XmlDomMapleData(myNode);
            ret.m_mImageDataDir = new DirectoryInfo($"{m_mImageDataDir}\\{Name}\\{path}").Parent.FullName;
            return ret;
        }


        public List<IMapleData> Children
        {
            get
            {
                var ret = new List<IMapleData>();
                var childNodes = m_mNode.ChildNodes;
                for (var i = 0; i < childNodes.Count; i++)
                {
                    var childNode = childNodes.Item(i);
                    if (childNode.NodeType == XmlNodeType.Element)
                    {
                        var child = new XmlDomMapleData(childNode);
                        child.m_mImageDataDir = new DirectoryInfo($"{m_mImageDataDir}\\{Name}").FullName;
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
                var attributes = m_mNode.Attributes;
                var type = GetType();
                switch (type)
                {
                    case MapleDataType.Double:
                    case MapleDataType.Float:
                    case MapleDataType.Int:
                    case MapleDataType.Short:
                    case MapleDataType.String:
                    case MapleDataType.Uol:
                    {
                        var value = attributes.GetNamedItem("value").Value;
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
                        var x = attributes.GetNamedItem("x").Value;
                        var y = attributes.GetNamedItem("y").Value;
                        return new Point(int.Parse(x), int.Parse(y));
                    }
                    case MapleDataType.Canvas:
                    {
                        var width = attributes.GetNamedItem("width").Value;
                        var height = attributes.GetNamedItem("height").Value;
                        return new FileStoredPngMapleCanvas(int.Parse(width), int.Parse(height),
                            $"{m_mImageDataDir}\\{Name}.png");
                    }
                }
                return null;
            }
        }


        public new MapleDataType GetType()
        {
            switch (m_mNode.Name)
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
                var parentNode = m_mNode.ParentNode;
                if (parentNode.NodeType == XmlNodeType.Document)
                {
                    return null; // can't traverse outside the img file - TODO is this a problem?
                }
                var parentData = new XmlDomMapleData(parentNode);
                parentData.m_mImageDataDir = new DirectoryInfo(m_mImageDataDir).Parent.FullName;
                return parentData;
            }
        }


        public string Name => m_mNode.Attributes.GetNamedItem("name").Value;
    }
}