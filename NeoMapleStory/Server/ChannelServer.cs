using System;
using System.Collections.Generic;
using System.Linq;
using NeoMapleStory.Core;
using NeoMapleStory.Game.Client;
using NeoMapleStory.Game.Data;
using NeoMapleStory.Game.Map;
using NeoMapleStory.Game.World;
using NeoMapleStory.Packet;

namespace NeoMapleStory.Server
{
    public sealed class ChannelServer : BaseServer
    {
        private readonly Dictionary<MapleSquadType, MapleSquad> m_mapleSquads =
            new Dictionary<MapleSquadType, MapleSquad>();

        public ChannelServer(int id)
        {
            MapFactory = new MapleMapFactory(MapleDataProviderFactory.GetDataProvider("Map.wz"),
                MapleDataProviderFactory.GetDataProvider("String.wz"));
            ChannelId = id;

            TimerManager.Instance.RepeatTask(RespawnMaps, 10*1000);
        }

        public int ChannelId { get; }
        public List<MapleCharacter> Characters { get; } = new List<MapleCharacter>();

        public int ExpRate { get; private set; } = 1;
        public int BossDropRate { get; private set; } = 1;
        public int DropRate { get; private set; } = 1;
        public int MesoRate { get; private set; } = 1;
        public int PetExpRate { get; private set; } = 1;

        public MapleMapFactory MapFactory { get; }
        public int UserLogged => ClientCount;
        public bool AllowMoreThanOne { get; set; }
        public bool AllowEnterCashShop { get; set; } = true;

        protected override void OnNewClientConnected(MapleClient client)
        {
            client.SendRaw(PacketCreator.Handshake(client.SendIv, client.RecvIv));
            client.HasHandShaked = true;
        }

        protected override void OnPacketHandlers()
        {
            Processor = new PacketProcessor("频道服务器");

            Processor.AppendHandler(RecvOpcodes.Pong, ChannelPacketHandlers.PONG);

            Processor.AppendHandler(RecvOpcodes.PlayerLoggedin, ChannelPacketHandlers.PLAYER_LOGGEDIN);
            Processor.AppendHandler(RecvOpcodes.PlayerUpdate, ChannelPacketHandlers.PLAYER_UPDATE);
            Processor.AppendHandler(RecvOpcodes.ChangeMapSpecial, ChannelPacketHandlers.CHANGE_MAP_SPECIAL);
            Processor.AppendHandler(RecvOpcodes.NpcAction, ChannelPacketHandlers.NPC_ACTION);
            Processor.AppendHandler(RecvOpcodes.MovePlayer, ChannelPacketHandlers.MOVE_PLAYER);
            Processor.AppendHandler(RecvOpcodes.ChangeMap, ChannelPacketHandlers.CHANGE_MAP);
            Processor.AppendHandler(RecvOpcodes.GeneralChat, ChannelPacketHandlers.GENERAL_CHAT);
            Processor.AppendHandler(RecvOpcodes.NpcTalk, ChannelPacketHandlers.NPC_TALK);
            Processor.AppendHandler(RecvOpcodes.NpcTalkMore, ChannelPacketHandlers.NPC_TALK_MORE);
            Processor.AppendHandler(RecvOpcodes.MoveLife, ChannelPacketHandlers.MOVE_LIFE);
            Processor.AppendHandler(RecvOpcodes.CloseRangeAttack, ChannelPacketHandlers.CLOSE_RANGE_ATTACK);
            Processor.AppendHandler(RecvOpcodes.TakeDamage, ChannelPacketHandlers.TAKE_DAMAGE);
            Processor.AppendHandler(RecvOpcodes.MagicAttack, ChannelPacketHandlers.MAGIC_ATTACK);
            Processor.AppendHandler(RecvOpcodes.ItemPickup, ChannelPacketHandlers.ITEM_PICKUP);
            Processor.AppendHandler(RecvOpcodes.DamageReactor, ChannelPacketHandlers.DAMAGE_REACTOR);
            Processor.AppendHandler(RecvOpcodes.DistributeAp, ChannelPacketHandlers.DISTRIBUTE_AP);
            Processor.AppendHandler(RecvOpcodes.DistributeAutoAp, ChannelPacketHandlers.DISTRIBUTE_AUTO_AP);
            Processor.AppendHandler(RecvOpcodes.DistributeSp, ChannelPacketHandlers.DISTRIBUTE_SP);
            Processor.AppendHandler(RecvOpcodes.HealOverTime, ChannelPacketHandlers.HEAL_OVERTIME);
            Processor.AppendHandler(RecvOpcodes.UseItem, ChannelPacketHandlers.USE_ITEM);
            Processor.AppendHandler(RecvOpcodes.SpecialMove, ChannelPacketHandlers.SPECIAL_MOVE);
            Processor.AppendHandler(RecvOpcodes.EnterCashShop, ChannelPacketHandlers.ENTER_CASHSHOP);
            Processor.AppendHandler(RecvOpcodes.EnterMts, ChannelPacketHandlers.ENTER_MTS);
            Processor.AppendHandler(RecvOpcodes.CashShop, ChannelPacketHandlers.CASHSHOP);
            Processor.AppendHandler(RecvOpcodes.ItemMove, ChannelPacketHandlers.ITEM_MOVE);
            Processor.AppendHandler(RecvOpcodes.TouchingCs, ChannelPacketHandlers.TOUCHING_CASHSHOP);
            Processor.AppendHandler(RecvOpcodes.UseChair, ChannelPacketHandlers.USE_CHAIR);
            Processor.AppendHandler(RecvOpcodes.CancelChair, ChannelPacketHandlers.CANCEL_CHAIR);
            Processor.AppendHandler(RecvOpcodes.UseUpgradeScroll, ChannelPacketHandlers.USE_UPGRADE_SCROLL);
            Processor.AppendHandler(RecvOpcodes.SpawnPet, ChannelPacketHandlers.SPAWN_PET);
            Processor.AppendHandler(RecvOpcodes.MovePet, ChannelPacketHandlers.MOVE_PET);
            Processor.AppendHandler(RecvOpcodes.CharInfoRequest, ChannelPacketHandlers.CHARINFO_REQUEST);
            Processor.AppendHandler(RecvOpcodes.PetLoot, ChannelPacketHandlers.PET_LOOT);
            Processor.AppendHandler(RecvOpcodes.PetAutoPot, ChannelPacketHandlers.PET_AUTO_POT);
            Processor.AppendHandler(RecvOpcodes.PetChat, ChannelPacketHandlers.PET_CHAT);
            Processor.AppendHandler(RecvOpcodes.PetCommand, ChannelPacketHandlers.PET_COMMAND);
            Processor.AppendHandler(RecvOpcodes.PetFood, ChannelPacketHandlers.PET_FOOD);
            Processor.AppendHandler(RecvOpcodes.UseReturnScroll, ChannelPacketHandlers.USE_RETURN_SCROLL);
            Processor.AppendHandler(RecvOpcodes.NpcShop, ChannelPacketHandlers.NPC_SHOP);
            Processor.AppendHandler(RecvOpcodes.ItemSort, ChannelPacketHandlers.ITEM_SORT);
            Processor.AppendHandler(RecvOpcodes.UseInnerPortal, ChannelPacketHandlers.USE_INNER_PORTAL);
        }

        public override bool Start()
        {
            Console.WriteLine($"正在启动 {Processor.Label} {ChannelId + 1}线 监听端口: {Config.Port}");
            var result = base.Start();
            if (result)
                Console.WriteLine($"{Processor.Label} {ChannelId + 1}线 启动成功");
            else
                Console.WriteLine($"{Processor.Label} {ChannelId + 1}线 启动失败");
            return result;
        }

        public override void Stop()
        {
            Console.WriteLine($"正在停止 {Processor.Label} {ChannelId + 1}线");
            base.Stop();
            Console.WriteLine($"{Processor.Label} {ChannelId + 1}线 已停止");
        }

        public List<MapleCharacter> GetPartyMembers(MapleParty party)
        {
            var partym = new List<MapleCharacter>();

            party.GetMembers().ForEach(partychar =>
            {
                if (partychar.ChannelId == ChannelId)
                {
                    // Make sure the thing doesn't get duplicate plays due to ccing bug.
                    var chr = Characters.FirstOrDefault(x => x.Name == partychar.CharacterName);
                    if (chr != null)
                    {
                        partym.Add(chr);
                    }
                }
            });
            return partym;
        }

        public MapleSquad GetMapleSquad(MapleSquadType type) => m_mapleSquads.ContainsKey(type) ? m_mapleSquads[type] : null;

        public bool AddMapleSquad(MapleSquad squad, MapleSquadType type)
        {
            if (!m_mapleSquads.ContainsKey(type))
            {
                m_mapleSquads.Remove(type);
                m_mapleSquads.Add(type, squad);
                return true;
            }
            return false;
        }

        public bool RemoveMapleSquad(MapleSquad squad, MapleSquadType type)
        {
            if (m_mapleSquads.ContainsKey(type))
            {
                if (m_mapleSquads[type] == squad)
                {
                    m_mapleSquads.Remove(type);
                    return true;
                }
            }
            return false;
        }

        private void RespawnMaps()
        {
            foreach (var map in MapFactory.Maps.Values)
            {
                map.Respawn();
            }
        }
    }
}