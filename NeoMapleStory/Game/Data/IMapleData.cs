using System.Collections.Generic;

namespace NeoMapleStory.Game.Data
{
    public interface IMapleData : IMapleDataEntity, IEnumerable<IMapleData>
    {
        //string Name { get; }

        object Data { get; }

        List<IMapleData> Children { get; }

        MapleDataType GetType();

        IMapleData GetChildByPath(string path);
    }
}