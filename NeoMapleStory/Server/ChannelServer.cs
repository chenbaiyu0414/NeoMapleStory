using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.World;
using NeoMapleStory.Packet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoMapleStory.Server
{
    
    public sealed class ChannelServer : BaseServer
    {
        public int ChannelId { get; private set; }
        public List<MapleCharacter> Characters { get; } = new List<MapleCharacter>();

        public int ExpRate { get; private set; }

        public MapleMapFactory MapFactory { get; private set; }
        public int UserLogged => ClientCount;
        public bool AllowMoreThanOne { get; set; }
        private Dictionary<MapleSquadType, MapleSquad> mapleSquads = new Dictionary<MapleSquadType, MapleSquad>();

        public ChannelServer(int id)
        {
            MapFactory= new MapleMapFactory(MapleDataProviderFactory.GetDataProvider("Map.wz"), MapleDataProviderFactory.GetDataProvider("String.wz"));
            ChannelId = id;
        }

        protected override void OnNewClientConnected(MapleClient client)
        {
            Console.WriteLine($"玩家{client.SessionID} 进入 频道服务器{ChannelId}");
            client.SendRaw(PacketCreator.Handshake(client.SendIv, client.RecvIv));
        }

        protected override void OnPacketHandlers()
        {
            MProcessor = new PacketProcessor("频道服务器");

            MProcessor.AppendHandler(RecvOpcodes.PlayerLoggedin, ChannelPacketHandlers.PLAYER_LOGGEDIN);
            MProcessor.AppendHandler(RecvOpcodes.PlayerUpdate, ChannelPacketHandlers.PLAYER_UPDATE);
            MProcessor.AppendHandler(RecvOpcodes.ChangeMapSpecial, ChannelPacketHandlers.CHANGE_MAP_SPECIAL);
            MProcessor.AppendHandler(RecvOpcodes.NpcAction, ChannelPacketHandlers.NPC_ACTION);
            MProcessor.AppendHandler(RecvOpcodes.MovePlayer, ChannelPacketHandlers.MOVE_PLAYER);
            MProcessor.AppendHandler(RecvOpcodes.ChangeMap, ChannelPacketHandlers.CHANGE_MAP);
            MProcessor.AppendHandler(RecvOpcodes.GeneralChat, ChannelPacketHandlers.GENERAL_CHAT);
            MProcessor.AppendHandler(RecvOpcodes.NpcTalk, ChannelPacketHandlers.NPC_TALK);
            MProcessor.AppendHandler(RecvOpcodes.MoveLife, ChannelPacketHandlers.MOVE_LIFE);
        }

        public override bool Start()
        {
            Console.WriteLine($"正在启动 {MProcessor.Label} {ChannelId + 1}线 监听端口: {Config.Port}");
            bool result = base.Start();
            if (result)
                Console.WriteLine($"{MProcessor.Label} {ChannelId + 1}线 启动成功");
            else
                Console.WriteLine($"{MProcessor.Label} {ChannelId + 1}线 启动失败");
            return result;
        }
        public override void Stop()
        {
            Console.WriteLine($"正在停止 {MProcessor.Label} {ChannelId + 1}线");
            base.Stop();
            Console.WriteLine($"{MProcessor.Label} {ChannelId + 1}线 已停止");
        }

        public List<MapleCharacter> GetPartyMembers(MapleParty party)
        {
            List<MapleCharacter> partym = new List<MapleCharacter>();

            party.GetMembers().ForEach(partychar =>
            {
                if (partychar.ChannelId == ChannelId)
                {
                    // Make sure the thing doesn't get duplicate plays due to ccing bug.
                    MapleCharacter chr = Characters.FirstOrDefault(x => x.Name == partychar.CharacterName);
                    if (chr != null)
                    {
                        partym.Add(chr);
                    }
                }
            });
            return partym;
        }

        public MapleSquad getMapleSquad(MapleSquadType type) => mapleSquads.ContainsKey(type) ? mapleSquads[type] : null;

        public bool addMapleSquad(MapleSquad squad, MapleSquadType type)
        {
            if (!mapleSquads.ContainsKey(type))
            {
                mapleSquads.Remove(type);
                mapleSquads.Add(type, squad);
                return true;
            }
            else {
                return false;
            }
        }

        public bool removeMapleSquad(MapleSquad squad, MapleSquadType type)
        {
            if (mapleSquads.ContainsKey(type))
            {
                if (mapleSquads[type] == squad)
                {
                    mapleSquads.Remove(type);
                    return true;
                }
            }
            return false;
        }
    }
}
