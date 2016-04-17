using System.Collections.Generic;

namespace NeoMapleStory.Game.Data
{
    public interface IMapleData : IMapleDataEntity, IEnumerable<IMapleData>
    {
        //string Name { get; }

        object Data { get; }

        MapleDataType GetType();

        List<IMapleData> Children { get; }

        IMapleData GetChildByPath(string path);

    }
}
